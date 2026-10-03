using Aurum.Api.Contracts.Walletspaces;
using Aurum.Application.Interfaces.Auth;
using Aurum.Domain.Entities.Workspace;
using Aurum.Domain.Enums;
using Aurum.Domain.Services;
using Aurum.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class WalletspacesController : ControllerBase
{
    private readonly AurumDbContext _db;
    private readonly ICurrentUser _currentUser;

    public WalletspacesController(AurumDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("walletspaces")]
    public async Task<ActionResult<IReadOnlyList<WalletspaceDto>>> GetWalletspaces(
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var spaces = await _db.Walletspaces
            .AsNoTracking()
            .Where(space => space.Members.Any(member => member.UserId == userId))
            .OrderBy(space => space.Name)
            .Select(space => new WalletspaceDto(
                space.Id,
                space.Name,
                space.Members.Where(member => member.UserId == userId)
                    .Select(member => member.Role).First(),
                space.Members.Count,
                space.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(spaces);
    }

    [HttpPost("walletspaces")]
    public async Task<ActionResult<WalletspaceDto>> CreateWalletspace(
        CreateWalletspaceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var space = new Walletspace(request.Name, _currentUser.UserId);
            _db.Walletspaces.Add(space);
            await _db.SaveChangesAsync(cancellationToken);

            var result = new WalletspaceDto(
                space.Id,
                space.Name,
                WalletspaceRole.Owner,
                1,
                space.CreatedAt);
            return CreatedAtAction(nameof(GetWalletspace), new { walletspaceId = space.Id }, result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message, Status = 400 });
        }
    }

    [HttpGet("walletspaces/{walletspaceId:guid}")]
    public async Task<ActionResult<WalletspaceDto>> GetWalletspace(
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        var space = await _db.Walletspaces
            .AsNoTracking()
            .Where(item => item.Id == walletspaceId)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.CreatedAt,
                MemberCount = item.Members.Count,
                Role = item.Members
                    .Where(member => member.UserId == _currentUser.UserId)
                    .Select(member => (WalletspaceRole?)member.Role)
                    .FirstOrDefault()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (space is null || space.Role is null)
            return NotFound();

        return Ok(new WalletspaceDto(
            space.Id,
            space.Name,
            space.Role.Value,
            space.MemberCount,
            space.CreatedAt));
    }

    [HttpPut("walletspaces/{walletspaceId:guid}")]
    public async Task<IActionResult> RenameWalletspace(
        Guid walletspaceId,
        RenameWalletspaceRequest request,
        CancellationToken cancellationToken)
    {
        var (space, role) = await GetSpaceForWriteAsync(walletspaceId, cancellationToken);
        if (space is null)
            return NotFound();
        if (!CanManageMembers(role))
            return Forbid();

        try
        {
            space.Rename(request.Name, _currentUser.UserId);
            await _db.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message, Status = 400 });
        }
    }

    [HttpDelete("walletspaces/{walletspaceId:guid}")]
    public async Task<IActionResult> DeleteWalletspace(
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        var (space, role) = await GetSpaceForWriteAsync(walletspaceId, cancellationToken);
        if (space is null)
            return NotFound();
        if (role != WalletspaceRole.Owner)
            return Forbid();

        _db.Walletspaces.Remove(space);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("walletspaces/{walletspaceId:guid}/accounts")]
    public async Task<ActionResult<IReadOnlyList<FinancialAccountDto>>> GetAccounts(
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();

        var accounts = await _db.FinancialAccounts
            .AsNoTracking()
            .Where(account => account.WalletspaceId == walletspaceId)
            .OrderBy(account => account.Name)
            .ToListAsync(cancellationToken);
        var accountIds = accounts.Select(account => account.Id).ToArray();
        var changes = await _db.FinancialTransactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.FinancialAccountId.HasValue &&
                accountIds.Contains(transaction.FinancialAccountId.Value) &&
                transaction.Status == FinancialTransactionStatus.Paid)
            .GroupBy(transaction => transaction.FinancialAccountId)
            .Select(group => new
            {
                AccountId = group.Key!.Value,
                BalanceChange = group.Sum(transaction =>
                    transaction.Type == FinancialTransactionType.Income
                        ? transaction.Amount
                        : -transaction.Amount)
            })
            .ToDictionaryAsync(item => item.AccountId, item => item.BalanceChange, cancellationToken);

        return Ok(accounts.Select(account => new FinancialAccountDto(
            account.Id,
            account.WalletspaceId,
            account.Name,
            account.Type,
            account.Institution,
            account.OpeningBalance,
            account.OpeningBalance + changes.GetValueOrDefault(account.Id))));
    }

    [HttpPost("walletspaces/{walletspaceId:guid}/accounts")]
    public async Task<ActionResult<FinancialAccountDto>> CreateAccount(
        Guid walletspaceId,
        SaveFinancialAccountRequest request,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();
        if (!CanManageFinancialData(role.Value))
            return Forbid();

        try
        {
            var account = new FinancialAccount(
                walletspaceId, request.Name, request.Type, request.OpeningBalance,
                request.Institution, _currentUser.UserId);
            _db.FinancialAccounts.Add(account);
            await _db.SaveChangesAsync(cancellationToken);
            return CreatedAtAction(
                nameof(GetAccounts),
                new { walletspaceId },
                new FinancialAccountDto(
                    account.Id,
                    account.WalletspaceId,
                    account.Name,
                    account.Type,
                    account.Institution,
                    account.OpeningBalance,
                    account.OpeningBalance));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message, Status = 400 });
        }
    }

    [HttpPut("walletspaces/{walletspaceId:guid}/accounts/{accountId:guid}")]
    public async Task<IActionResult> UpdateAccount(
        Guid walletspaceId,
        Guid accountId,
        SaveFinancialAccountRequest request,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();
        if (!CanManageFinancialData(role.Value))
            return Forbid();

        var account = await _db.FinancialAccounts.SingleOrDefaultAsync(
            item => item.Id == accountId && item.WalletspaceId == walletspaceId,
            cancellationToken);
        if (account is null)
            return NotFound();

        try
        {
            account.SetDetails(
                request.Name,
                request.Type,
                request.OpeningBalance,
                request.Institution,
                _currentUser.UserId);
            await _db.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message, Status = 400 });
        }
    }

    [HttpDelete("walletspaces/{walletspaceId:guid}/accounts/{accountId:guid}")]
    public async Task<IActionResult> DeleteAccount(
        Guid walletspaceId,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();
        if (!CanManageFinancialData(role.Value))
            return Forbid();

        var account = await _db.FinancialAccounts.SingleOrDefaultAsync(
            item => item.Id == accountId && item.WalletspaceId == walletspaceId,
            cancellationToken);
        if (account is null)
            return NotFound();
        if (await _db.FinancialTransactions.AnyAsync(
                transaction => transaction.FinancialAccountId == accountId,
                cancellationToken))
            return Conflict(new ProblemDetails
            {
                Title = "A conta possui lançamentos vinculados e não pode ser removida.",
                Status = 409
            });

        _db.FinancialAccounts.Remove(account);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("walletspaces/{walletspaceId:guid}/cards")]
    public async Task<ActionResult<IReadOnlyList<CreditCardDto>>> GetCards(
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();

        var cards = await _db.CreditCards
            .AsNoTracking()
            .Where(card => card.WalletspaceId == walletspaceId)
            .OrderBy(card => card.Name)
            .Select(card => new
            {
                Card = card,
                Outstanding = _db.FinancialTransactions
                    .Where(transaction =>
                        transaction.CreditCardId == card.Id &&
                        transaction.Type == FinancialTransactionType.Expense &&
                        transaction.Status == FinancialTransactionStatus.Pending)
                    .Sum(transaction => (decimal?)transaction.Amount) ?? 0m
            })
            .ToListAsync(cancellationToken);

        return Ok(cards.Select(item => new CreditCardDto(
            item.Card.Id,
            item.Card.WalletspaceId,
            item.Card.Name,
            item.Card.LastFourDigits,
            item.Card.CreditLimit,
            item.Card.ClosingDay,
            item.Card.DueDay,
            item.Outstanding)));
    }

    [HttpPost("walletspaces/{walletspaceId:guid}/cards")]
    public async Task<ActionResult<CreditCardDto>> CreateCard(
        Guid walletspaceId,
        SaveCreditCardRequest request,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();
        if (!CanManageFinancialData(role.Value))
            return Forbid();

        try
        {
            var card = new CreditCard(
                walletspaceId, request.Name, request.LastFourDigits,
                request.CreditLimit, request.ClosingDay, request.DueDay,
                _currentUser.UserId);
            _db.CreditCards.Add(card);
            await _db.SaveChangesAsync(cancellationToken);
            return CreatedAtAction(
                nameof(GetCards),
                new { walletspaceId },
                new CreditCardDto(
                    card.Id, walletspaceId, card.Name, card.LastFourDigits,
                    card.CreditLimit, card.ClosingDay, card.DueDay, 0));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message, Status = 400 });
        }
    }

    [HttpPut("walletspaces/{walletspaceId:guid}/cards/{cardId:guid}")]
    public async Task<IActionResult> UpdateCard(
        Guid walletspaceId,
        Guid cardId,
        SaveCreditCardRequest request,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();
        if (!CanManageFinancialData(role.Value))
            return Forbid();

        var card = await _db.CreditCards.SingleOrDefaultAsync(
            item => item.Id == cardId && item.WalletspaceId == walletspaceId,
            cancellationToken);
        if (card is null)
            return NotFound();

        try
        {
            card.SetDetails(
                request.Name, request.LastFourDigits, request.CreditLimit,
                request.ClosingDay, request.DueDay, _currentUser.UserId);
            await _db.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message, Status = 400 });
        }
    }

    [HttpDelete("walletspaces/{walletspaceId:guid}/cards/{cardId:guid}")]
    public async Task<IActionResult> DeleteCard(
        Guid walletspaceId,
        Guid cardId,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();
        if (!CanManageFinancialData(role.Value))
            return Forbid();

        var card = await _db.CreditCards.SingleOrDefaultAsync(
            item => item.Id == cardId && item.WalletspaceId == walletspaceId,
            cancellationToken);
        if (card is null)
            return NotFound();
        if (await _db.FinancialTransactions.AnyAsync(
                transaction => transaction.CreditCardId == cardId,
                cancellationToken))
            return Conflict(new ProblemDetails
            {
                Title = "O cartão possui lançamentos vinculados e não pode ser removido.",
                Status = 409
            });

        _db.CreditCards.Remove(card);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("walletspaces/{walletspaceId:guid}/transactions")]
    public async Task<ActionResult<IReadOnlyList<FinancialTransactionDto>>> GetTransactions(
        Guid walletspaceId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();
        if (from > to)
            return BadRequest(new ProblemDetails { Title = "O início do período deve anteceder o fim.", Status = 400 });

        var query = _db.FinancialTransactions.AsNoTracking()
            .Where(transaction => transaction.WalletspaceId == walletspaceId);
        if (from.HasValue)
            query = query.Where(transaction => transaction.TransactionDate >= from.Value);
        if (to.HasValue)
            query = query.Where(transaction => transaction.TransactionDate <= to.Value);

        var transactions = await query
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenBy(transaction => transaction.Description)
            .Select(transaction => new FinancialTransactionDto(
                transaction.Id,
                transaction.WalletspaceId,
                transaction.FinancialAccountId,
                transaction.CreditCardId,
                transaction.Description,
                transaction.Category,
                transaction.Amount,
                transaction.Type,
                transaction.Status,
                transaction.TransactionDate,
                transaction.DueDate,
                transaction.Recurrence,
                transaction.SeriesId,
                transaction.InstallmentNumber,
                transaction.InstallmentCount))
            .ToListAsync(cancellationToken);

        return Ok(transactions);
    }

    [HttpPost("walletspaces/{walletspaceId:guid}/transactions")]
    public async Task<ActionResult<FinancialTransactionDto>> CreateTransaction(
        Guid walletspaceId,
        SaveFinancialTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();
        if (!CanWriteTransactions(role.Value))
            return Forbid();

        var resourceError = await ValidateTransactionResourceAsync(
            walletspaceId, request.FinancialAccountId, request.CreditCardId,
            request.Type, cancellationToken);
        if (resourceError is not null)
            return BadRequest(new ProblemDetails { Title = resourceError, Status = 400 });

        try
        {
            CreditCard? card = null;
            if (request.CreditCardId.HasValue)
                card = await _db.CreditCards.AsNoTracking().SingleAsync(
                    item => item.Id == request.CreditCardId.Value,
                    cancellationToken);

            var recurrence = request.Recurrence;
            if (!Enum.IsDefined(recurrence))
                return BadRequest(new ProblemDetails { Title = "Recorrência inválida.", Status = 400 });
            if (recurrence != FinancialTransactionRecurrence.None && request.Occurrences is null)
                return BadRequest(new ProblemDetails
                {
                    Title = "Informe a quantidade de meses/parcelas.",
                    Status = 400
                });

            var plan = TransactionScheduler.Plan(
                recurrence,
                request.Occurrences ?? 1,
                request.Amount,
                request.AmountIsPerInstallment,
                request.TransactionDate,
                request.DueDate,
                card?.ClosingDay,
                card?.DueDay);
            var seriesId = Guid.NewGuid();
            var created = new List<FinancialTransaction>(plan.Count);
            foreach (var occurrence in plan)
            {
                var transaction = new FinancialTransaction(
                    walletspaceId,
                    request.FinancialAccountId,
                    request.CreditCardId,
                    request.Description,
                    request.Category,
                    occurrence.Amount,
                    request.Type,
                    occurrence.Number == 1 ? request.Status : FinancialTransactionStatus.Pending,
                    occurrence.TransactionDate,
                    occurrence.DueDate,
                    _currentUser.UserId);
                if (recurrence != FinancialTransactionRecurrence.None)
                    transaction.MarkAsSeries(recurrence, seriesId, occurrence.Number, plan.Count);
                created.Add(transaction);
            }

            _db.FinancialTransactions.AddRange(created);
            await _db.SaveChangesAsync(cancellationToken);
            return CreatedAtAction(
                nameof(GetTransactions),
                new { walletspaceId },
                ToDto(created[0]));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message, Status = 400 });
        }
    }

    [HttpPut("walletspaces/{walletspaceId:guid}/transactions/{transactionId:guid}")]
    public async Task<IActionResult> UpdateTransaction(
        Guid walletspaceId,
        Guid transactionId,
        SaveFinancialTransactionRequest request,
        [FromQuery] TransactionSeriesScope scope,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();
        if (!CanWriteTransactions(role.Value))
            return Forbid();

        var transaction = await _db.FinancialTransactions.SingleOrDefaultAsync(
            item => item.Id == transactionId && item.WalletspaceId == walletspaceId,
            cancellationToken);
        if (transaction is null)
            return NotFound();

        var resourceError = await ValidateTransactionResourceAsync(
            walletspaceId, request.FinancialAccountId, request.CreditCardId,
            request.Type, cancellationToken);
        if (resourceError is not null)
            return BadRequest(new ProblemDetails { Title = resourceError, Status = 400 });

        try
        {
            if (scope != TransactionSeriesScope.Single && transaction.SeriesId.HasValue)
            {
                var followers = await _db.FinancialTransactions
                    .Where(item =>
                        item.WalletspaceId == walletspaceId &&
                        item.SeriesId == transaction.SeriesId &&
                        item.Id != transaction.Id &&
                        item.TransactionDate >= transaction.TransactionDate)
                    .ToListAsync(cancellationToken);
                // Cada ocorrência mantém suas próprias datas e situação.
                foreach (var follower in followers)
                    follower.SetDetails(
                        request.FinancialAccountId,
                        request.CreditCardId,
                        request.Description,
                        request.Category,
                        request.Amount,
                        request.Type,
                        follower.Status,
                        follower.TransactionDate,
                        follower.DueDate,
                        _currentUser.UserId);
            }

            transaction.SetDetails(
                request.FinancialAccountId,
                request.CreditCardId,
                request.Description,
                request.Category,
                request.Amount,
                request.Type,
                request.Status,
                request.TransactionDate,
                request.DueDate,
                _currentUser.UserId);
            await _db.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message, Status = 400 });
        }
    }

    [HttpDelete("walletspaces/{walletspaceId:guid}/transactions/{transactionId:guid}")]
    public async Task<IActionResult> DeleteTransaction(
        Guid walletspaceId,
        Guid transactionId,
        [FromQuery] TransactionSeriesScope scope,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();
        if (!CanWriteTransactions(role.Value))
            return Forbid();

        var transaction = await _db.FinancialTransactions.SingleOrDefaultAsync(
            item => item.Id == transactionId && item.WalletspaceId == walletspaceId,
            cancellationToken);
        if (transaction is null)
            return NotFound();

        if (scope != TransactionSeriesScope.Single && transaction.SeriesId.HasValue)
        {
            var seriesItems = await _db.FinancialTransactions
                .Where(item =>
                    item.WalletspaceId == walletspaceId &&
                    item.SeriesId == transaction.SeriesId &&
                    (scope == TransactionSeriesScope.All ||
                        item.TransactionDate >= transaction.TransactionDate))
                .ToListAsync(cancellationToken);
            _db.FinancialTransactions.RemoveRange(seriesItems);
        }
        else
        {
            _db.FinancialTransactions.Remove(transaction);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("walletspaces/{walletspaceId:guid}/reports/summary")]
    public async Task<ActionResult<FinancialSummaryDto>> GetReportSummary(
        Guid walletspaceId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return NotFound();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = from ?? new DateOnly(today.Year, today.Month, 1);
        var end = to ?? today;
        if (start > end)
            return BadRequest(new ProblemDetails { Title = "O início do período deve anteceder o fim.", Status = 400 });
        if (end.DayNumber - start.DayNumber > 366)
            return BadRequest(new ProblemDetails { Title = "O período do relatório não pode exceder 12 meses.", Status = 400 });

        var transactions = await _db.FinancialTransactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.WalletspaceId == walletspaceId &&
                transaction.TransactionDate >= start &&
                transaction.TransactionDate <= end)
            .ToListAsync(cancellationToken);
        var paid = transactions.Where(item => item.Status == FinancialTransactionStatus.Paid);
        var income = paid.Where(item => item.Type == FinancialTransactionType.Income).Sum(item => item.Amount);
        var expense = paid.Where(item => item.Type == FinancialTransactionType.Expense).Sum(item => item.Amount);
        var categories = paid
            .GroupBy(item => new { Category = item.Category ?? "Sem categoria", item.Type })
            .Select(group => new CategorySummaryDto(group.Key.Category, group.Key.Type, group.Sum(item => item.Amount)))
            .OrderByDescending(item => item.Total)
            .ToArray();

        return Ok(new FinancialSummaryDto(
            walletspaceId,
            start,
            end,
            income,
            expense,
            income - expense,
            transactions.Count(item => item.Status == FinancialTransactionStatus.Pending),
            categories));
    }

    [HttpGet("walletspaces/{walletspaceId:guid}/members")]
    public async Task<ActionResult<IReadOnlyList<WalletspaceMemberDto>>> GetMembers(
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        if (await GetRoleAsync(walletspaceId, cancellationToken) is null)
            return NotFound();

        var members = await (
            from member in _db.WalletspaceMembers.AsNoTracking()
            join user in _db.Users.AsNoTracking() on member.UserId equals user.Id
            where member.WalletspaceId == walletspaceId
            orderby member.JoinedAt
            select new WalletspaceMemberDto(
                member.UserId,
                user.Email ?? string.Empty,
                user.FullName,
                member.Role,
                member.JoinedAt))
            .ToListAsync(cancellationToken);

        return Ok(members);
    }

    [HttpPost("walletspaces/{walletspaceId:guid}/members")]
    public async Task<ActionResult<WalletspaceMemberDto>> AddMember(
        Guid walletspaceId,
        AddWalletspaceMemberRequest request,
        CancellationToken cancellationToken)
    {
        var actorRole = await GetRoleAsync(walletspaceId, cancellationToken);
        if (actorRole is null)
            return NotFound();
        if (!CanManageMembers(actorRole.Value))
            return Forbid();
        if (request.Role == WalletspaceRole.Owner ||
            (request.Role == WalletspaceRole.Admin && actorRole != WalletspaceRole.Owner) ||
            !Enum.IsDefined(request.Role))
            return BadRequest(new ProblemDetails
            {
                Title = "Seu perfil não pode atribuir essa permissão.",
                Status = 400
            });

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(
            item => item.NormalizedEmail == normalizedEmail,
            cancellationToken);
        if (user is null)
            return NotFound(new ProblemDetails
            {
                Title = "Não encontramos uma conta com esse e-mail. A pessoa precisa criar uma conta antes de ser adicionada.",
                Status = 404
            });

        if (await _db.WalletspaceMembers.AnyAsync(
                member => member.WalletspaceId == walletspaceId && member.UserId == user.Id,
                cancellationToken))
            return Conflict(new ProblemDetails { Title = "Essa pessoa já faz parte do Walletspace.", Status = 409 });

        var member = new WalletspaceMember(walletspaceId, user.Id, request.Role);
        _db.WalletspaceMembers.Add(member);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetMembers),
            new { walletspaceId },
            new WalletspaceMemberDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                member.Role,
                member.JoinedAt));
    }

    [HttpPut("walletspaces/{walletspaceId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> ChangeMemberRole(
        Guid walletspaceId,
        Guid userId,
        ChangeWalletspaceMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var actorRole = await GetRoleAsync(walletspaceId, cancellationToken);
        if (actorRole is null)
            return NotFound();
        if (!CanManageMembers(actorRole.Value))
            return Forbid();
        if (!Enum.IsDefined(request.Role) ||
            request.Role == WalletspaceRole.Owner ||
            (request.Role == WalletspaceRole.Admin && actorRole != WalletspaceRole.Owner))
            return BadRequest(new ProblemDetails { Title = "Seu perfil não pode atribuir essa permissão.", Status = 400 });

        var member = await _db.WalletspaceMembers.SingleOrDefaultAsync(
            item => item.WalletspaceId == walletspaceId && item.UserId == userId,
            cancellationToken);
        if (member is null)
            return NotFound();
        if (member.Role == WalletspaceRole.Owner)
            return Conflict(new ProblemDetails { Title = "A permissão do proprietário não pode ser alterada.", Status = 409 });
        if (member.Role == WalletspaceRole.Admin && actorRole != WalletspaceRole.Owner)
            return Forbid();

        member.ChangeRole(request.Role, _currentUser.UserId);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("walletspaces/{walletspaceId:guid}/members/transfer-ownership")]
    public async Task<IActionResult> TransferOwnership(
        Guid walletspaceId,
        TransferOwnershipRequest request,
        CancellationToken cancellationToken)
    {
        var actorRole = await GetRoleAsync(walletspaceId, cancellationToken);
        if (actorRole is null)
            return NotFound();
        if (actorRole != WalletspaceRole.Owner)
            return Forbid();
        if (request.NewOwnerId == _currentUser.UserId)
            return BadRequest(new ProblemDetails { Title = "Você já é o proprietário.", Status = 400 });

        var members = await _db.WalletspaceMembers
            .Where(item =>
                item.WalletspaceId == walletspaceId &&
                (item.UserId == request.NewOwnerId || item.UserId == _currentUser.UserId))
            .ToListAsync(cancellationToken);
        var target = members.SingleOrDefault(item => item.UserId == request.NewOwnerId);
        var current = members.Single(item => item.UserId == _currentUser.UserId);
        if (target is null)
            return NotFound(new ProblemDetails { Title = "A pessoa escolhida não faz parte do Walletspace.", Status = 404 });

        target.ChangeRole(WalletspaceRole.Owner, _currentUser.UserId);
        current.ChangeRole(WalletspaceRole.Admin, _currentUser.UserId);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("walletspaces/{walletspaceId:guid}/leave")]
    public async Task<IActionResult> LeaveWalletspace(
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        var member = await _db.WalletspaceMembers.SingleOrDefaultAsync(
            item => item.WalletspaceId == walletspaceId && item.UserId == _currentUser.UserId,
            cancellationToken);
        if (member is null)
            return NotFound();
        if (member.Role == WalletspaceRole.Owner)
            return Conflict(new ProblemDetails
            {
                Title = "O proprietário precisa transferir a propriedade ou excluir o Walletspace antes de sair.",
                Status = 409
            });

        _db.WalletspaceMembers.Remove(member);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("walletspaces/{walletspaceId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(
        Guid walletspaceId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var actorRole = await GetRoleAsync(walletspaceId, cancellationToken);
        if (actorRole is null)
            return NotFound();
        if (!CanManageMembers(actorRole.Value))
            return Forbid();

        var member = await _db.WalletspaceMembers.SingleOrDefaultAsync(
            item => item.WalletspaceId == walletspaceId && item.UserId == userId,
            cancellationToken);
        if (member is null)
            return NotFound();
        if (member.Role == WalletspaceRole.Owner)
            return Conflict(new ProblemDetails { Title = "O proprietário não pode ser removido do próprio Walletspace.", Status = 409 });
        if (member.Role == WalletspaceRole.Admin && actorRole != WalletspaceRole.Owner)
            return Forbid();

        _db.WalletspaceMembers.Remove(member);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("dashboard/summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var spaces = await _db.Walletspaces.AsNoTracking()
            .Where(space => space.Members.Any(member => member.UserId == userId))
            .OrderBy(space => space.Name)
            .Select(space => new WalletspaceDto(
                space.Id,
                space.Name,
                space.Members.Where(member => member.UserId == userId)
                    .Select(member => member.Role).First(),
                space.Members.Count,
                space.CreatedAt))
            .ToListAsync(cancellationToken);
        var spaceIds = spaces.Select(space => space.Id).ToArray();
        var accounts = await _db.FinancialAccounts.AsNoTracking()
            .Where(account => spaceIds.Contains(account.WalletspaceId))
            .Select(account => new { account.Id, account.OpeningBalance })
            .ToListAsync(cancellationToken);
        var accountIds = accounts.Select(account => account.Id).ToArray();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = new DateOnly(today.Year, today.Month, 1);
        var transactions = await _db.FinancialTransactions.AsNoTracking()
            .Where(transaction =>
                spaceIds.Contains(transaction.WalletspaceId) &&
                transaction.Status == FinancialTransactionStatus.Paid)
            .Select(transaction => new
            {
                transaction.FinancialAccountId,
                transaction.TransactionDate,
                transaction.Type,
                transaction.Amount
            })
            .ToListAsync(cancellationToken);

        var accountBalance = accounts.Sum(account => account.OpeningBalance) +
            transactions
                .Where(item => item.FinancialAccountId.HasValue && accountIds.Contains(item.FinancialAccountId.Value))
                .Sum(item => item.Type == FinancialTransactionType.Income ? item.Amount : -item.Amount);
        var monthTransactions = transactions.Where(item => item.TransactionDate >= start && item.TransactionDate <= today);
        return Ok(new DashboardSummaryDto(
            spaces.Count,
            accountBalance,
            monthTransactions.Where(item => item.Type == FinancialTransactionType.Income).Sum(item => item.Amount),
            monthTransactions.Where(item => item.Type == FinancialTransactionType.Expense).Sum(item => item.Amount),
            spaces));
    }

    private async Task<WalletspaceRole?> GetRoleAsync(
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        return await _db.WalletspaceMembers.AsNoTracking()
            .Where(member =>
                member.WalletspaceId == walletspaceId &&
                member.UserId == _currentUser.UserId)
            .Select(member => (WalletspaceRole?)member.Role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<(Walletspace? Space, WalletspaceRole Role)> GetSpaceForWriteAsync(
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return (null, default);

        var space = await _db.Walletspaces.SingleOrDefaultAsync(
            item => item.Id == walletspaceId,
            cancellationToken);
        return (space, role.Value);
    }

    private async Task<string?> ValidateTransactionResourceAsync(
        Guid walletspaceId,
        Guid? accountId,
        Guid? cardId,
        FinancialTransactionType type,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(type))
            return "Tipo de lançamento inválido.";
        if (accountId.HasValue &&
            !await _db.FinancialAccounts.AnyAsync(
                account => account.Id == accountId && account.WalletspaceId == walletspaceId,
                cancellationToken))
            return "A conta selecionada não pertence a este Walletspace.";
        if (cardId.HasValue &&
            !await _db.CreditCards.AnyAsync(
                card => card.Id == cardId && card.WalletspaceId == walletspaceId,
                cancellationToken))
            return "O cartão selecionado não pertence a este Walletspace.";
        if (cardId.HasValue && type != FinancialTransactionType.Expense)
            return "Lançamentos em cartão devem ser despesas.";
        return null;
    }

    private static FinancialTransactionDto ToDto(FinancialTransaction transaction) =>
        new(
            transaction.Id,
            transaction.WalletspaceId,
            transaction.FinancialAccountId,
            transaction.CreditCardId,
            transaction.Description,
            transaction.Category,
            transaction.Amount,
            transaction.Type,
            transaction.Status,
            transaction.TransactionDate,
            transaction.DueDate,
            transaction.Recurrence,
            transaction.SeriesId,
            transaction.InstallmentNumber,
            transaction.InstallmentCount);

    private static bool CanWriteTransactions(WalletspaceRole role) =>
        role is WalletspaceRole.Owner or WalletspaceRole.Admin or
            WalletspaceRole.Manager or WalletspaceRole.Member;

    private static bool CanManageFinancialData(WalletspaceRole role) =>
        role is WalletspaceRole.Owner or WalletspaceRole.Admin or WalletspaceRole.Manager;

    private static bool CanManageMembers(WalletspaceRole role) =>
        role is WalletspaceRole.Owner or WalletspaceRole.Admin;
}
