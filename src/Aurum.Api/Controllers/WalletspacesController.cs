using Aurum.Api.Contracts.Walletspaces;
using Aurum.Application.Financial;
using Aurum.Application.Interfaces.Auth;
using Aurum.Domain.Entities.Workspace;
using Aurum.Domain.Enums;
using Aurum.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class WalletspacesController(
    AurumDbContext db,
    ICurrentUser currentUser,
    IFinancialUseCases financialUseCases) : ControllerBase
{
    [HttpGet("walletspaces")]
    public async Task<ActionResult<IReadOnlyList<WalletspaceDto>>> GetWalletspaces(
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        var spaces = await db.Walletspaces
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
            var space = new Walletspace(request.Name, currentUser.UserId);
            db.Walletspaces.Add(space);
            await db.SaveChangesAsync(cancellationToken);
            var result = new WalletspaceDto(
                space.Id, space.Name, WalletspaceRole.Owner, 1, space.CreatedAt);
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
        var space = await db.Walletspaces
            .AsNoTracking()
            .Where(item => item.Id == walletspaceId)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.CreatedAt,
                MemberCount = item.Members.Count,
                Role = item.Members
                    .Where(member => member.UserId == currentUser.UserId)
                    .Select(member => (WalletspaceRole?)member.Role)
                    .FirstOrDefault()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (space is null || space.Role is null)
            return NotFound();
        return Ok(new WalletspaceDto(
            space.Id, space.Name, space.Role.Value, space.MemberCount, space.CreatedAt));
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
            space.Rename(request.Name, currentUser.UserId);
            await db.SaveChangesAsync(cancellationToken);
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

        db.Walletspaces.Remove(space);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("walletspaces/{walletspaceId:guid}/members")]
    public async Task<ActionResult<IReadOnlyList<WalletspaceMemberDto>>> GetMembers(
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        if (await GetRoleAsync(walletspaceId, cancellationToken) is null)
            return NotFound();

        var members = await (
            from member in db.WalletspaceMembers.AsNoTracking()
            join user in db.Users.AsNoTracking() on member.UserId equals user.Id
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
        var (space, actorRole) = await GetSpaceForWriteAsync(walletspaceId, cancellationToken);
        if (space is null)
            return NotFound();
        if (!CanManageMembers(actorRole))
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
        var user = await db.Users.SingleOrDefaultAsync(
            item => item.NormalizedEmail == normalizedEmail,
            cancellationToken);
        if (user is null)
            return NotFound(new ProblemDetails
            {
                Title = "Não encontramos uma conta com esse e-mail. A pessoa precisa criar uma conta antes de ser adicionada.",
                Status = 404
            });
        if (await db.WalletspaceMembers.AnyAsync(
                member => member.WalletspaceId == walletspaceId && member.UserId == user.Id,
                cancellationToken))
            return Conflict(new ProblemDetails
            {
                Title = "Essa pessoa já faz parte do Walletspace.",
                Status = 409
            });

        var member = new WalletspaceMember(walletspaceId, user.Id, request.Role);
        try
        {
            space.AddMember(member);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message, Status = 409 });
        }
        db.WalletspaceMembers.Add(member);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(
            nameof(GetMembers),
            new { walletspaceId },
            new WalletspaceMemberDto(
                user.Id, user.Email ?? string.Empty, user.FullName, member.Role, member.JoinedAt));
    }

    [HttpPut("walletspaces/{walletspaceId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> ChangeMemberRole(
        Guid walletspaceId,
        Guid userId,
        ChangeWalletspaceMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var (space, actorRole) = await GetSpaceForWriteAsync(walletspaceId, cancellationToken);
        if (space is null)
            return NotFound();
        if (!CanManageMembers(actorRole))
            return Forbid();
        if (!Enum.IsDefined(request.Role) ||
            request.Role == WalletspaceRole.Owner ||
            (request.Role == WalletspaceRole.Admin && actorRole != WalletspaceRole.Owner))
            return BadRequest(new ProblemDetails
            {
                Title = "Seu perfil não pode atribuir essa permissão.",
                Status = 400
            });

        var target = space.Members.SingleOrDefault(item => item.UserId == userId);
        if (target is null)
            return NotFound();
        if (target.Role == WalletspaceRole.Owner)
            return Conflict(new ProblemDetails { Title = "A permissão do proprietário não pode ser alterada.", Status = 409 });
        if (target.Role == WalletspaceRole.Admin && actorRole != WalletspaceRole.Owner)
            return Forbid();

        space.ChangeMemberRole(userId, request.Role, currentUser.UserId);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("walletspaces/{walletspaceId:guid}/members/transfer-ownership")]
    public async Task<IActionResult> TransferOwnership(
        Guid walletspaceId,
        TransferOwnershipRequest request,
        CancellationToken cancellationToken)
    {
        var (space, actorRole) = await GetSpaceForWriteAsync(walletspaceId, cancellationToken);
        if (space is null)
            return NotFound();
        if (actorRole != WalletspaceRole.Owner)
            return Forbid();
        if (request.NewOwnerId == currentUser.UserId)
            return BadRequest(new ProblemDetails { Title = "Você já é o proprietário.", Status = 400 });

        try
        {
            space.TransferOwnership(request.NewOwnerId, currentUser.UserId);
            await db.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return NotFound(new ProblemDetails { Title = exception.Message, Status = 404 });
        }
    }

    [HttpPost("walletspaces/{walletspaceId:guid}/leave")]
    public async Task<IActionResult> LeaveWalletspace(
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        var member = await db.WalletspaceMembers.SingleOrDefaultAsync(
            item => item.WalletspaceId == walletspaceId && item.UserId == currentUser.UserId,
            cancellationToken);
        if (member is null)
            return NotFound();
        if (member.Role == WalletspaceRole.Owner)
            return Conflict(new ProblemDetails
            {
                Title = "O proprietário precisa transferir a propriedade ou excluir o Walletspace antes de sair.",
                Status = 409
            });

        db.WalletspaceMembers.Remove(member);
        await db.SaveChangesAsync(cancellationToken);
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

        var member = await db.WalletspaceMembers.SingleOrDefaultAsync(
            item => item.WalletspaceId == walletspaceId && item.UserId == userId,
            cancellationToken);
        if (member is null)
            return NotFound();
        if (member.Role == WalletspaceRole.Owner)
            return Conflict(new ProblemDetails
            {
                Title = "O proprietário não pode ser removido do próprio Walletspace.",
                Status = 409
            });
        if (member.Role == WalletspaceRole.Admin && actorRole != WalletspaceRole.Owner)
            return Forbid();

        db.WalletspaceMembers.Remove(member);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("dashboard/summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        var spaces = await db.Walletspaces.AsNoTracking()
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
        var personal = await financialUseCases.GetPersonalDashboardAsync(userId, cancellationToken);
        return Ok(new DashboardSummaryDto(
            spaces.Count,
            personal.AccountBalance,
            personal.IncomeThisMonth,
            personal.ExpenseThisMonth,
            spaces));
    }

    private Task<WalletspaceRole?> GetRoleAsync(
        Guid walletspaceId,
        CancellationToken cancellationToken) =>
        db.WalletspaceMembers.AsNoTracking()
            .Where(member =>
                member.WalletspaceId == walletspaceId &&
                member.UserId == currentUser.UserId)
            .Select(member => (WalletspaceRole?)member.Role)
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<(Walletspace? Space, WalletspaceRole Role)> GetSpaceForWriteAsync(
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(walletspaceId, cancellationToken);
        if (role is null)
            return (null, default);

        var space = await db.Walletspaces.Include(item => item.Members).SingleOrDefaultAsync(
            item => item.Id == walletspaceId,
            cancellationToken);
        return (space, role.Value);
    }

    private static bool CanManageMembers(WalletspaceRole role) =>
        role is WalletspaceRole.Owner or WalletspaceRole.Admin;
}
