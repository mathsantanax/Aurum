using Aurum.Api.Contracts.Walletspaces;
using Aurum.Application.Financial;
using Aurum.Application.Interfaces.Auth;
using Aurum.Domain.Entities.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class FinancialController(
    IFinancialUseCases financialUseCases,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("accounts")]
    public Task<IActionResult> GetAccounts(CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var accounts = await financialUseCases.ListAccountsAsync(currentUser.UserId, cancellationToken);
            return Ok(accounts.Select(ToDto).ToArray());
        });

    [HttpPost("accounts")]
    public Task<IActionResult> CreateAccount(
        SaveFinancialAccountRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var account = await financialUseCases.CreateAccountAsync(
                currentUser.UserId,
                new FinancialAccountInput(request.Name, request.Type, request.OpeningBalance, request.Institution),
                cancellationToken);
            return CreatedAtAction(nameof(GetAccounts), ToDto(account));
        });

    [HttpPut("accounts/{accountId:guid}")]
    public Task<IActionResult> UpdateAccount(
        Guid accountId,
        SaveFinancialAccountRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            await financialUseCases.UpdateAccountAsync(
                currentUser.UserId,
                accountId,
                new FinancialAccountInput(request.Name, request.Type, request.OpeningBalance, request.Institution),
                cancellationToken);
            return NoContent();
        });

    [HttpDelete("accounts/{accountId:guid}")]
    public Task<IActionResult> DeleteAccount(Guid accountId, CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            await financialUseCases.DeleteAccountAsync(currentUser.UserId, accountId, cancellationToken);
            return NoContent();
        });

    [HttpGet("cards")]
    public Task<IActionResult> GetCards(CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var cards = await financialUseCases.ListCreditCardsAsync(currentUser.UserId, cancellationToken);
            return Ok(cards.Select(ToDto).ToArray());
        });

    [HttpPost("cards")]
    public Task<IActionResult> CreateCard(
        SaveCreditCardRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var card = await financialUseCases.CreateCreditCardAsync(
                currentUser.UserId,
                new CreditCardInput(
                    request.Name, request.LastFourDigits, request.CreditLimit,
                    request.ClosingDay, request.DueDay),
                cancellationToken);
            return CreatedAtAction(nameof(GetCards), ToDto(card));
        });

    [HttpPut("cards/{cardId:guid}")]
    public Task<IActionResult> UpdateCard(
        Guid cardId,
        SaveCreditCardRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            await financialUseCases.UpdateCreditCardAsync(
                currentUser.UserId,
                cardId,
                new CreditCardInput(
                    request.Name, request.LastFourDigits, request.CreditLimit,
                    request.ClosingDay, request.DueDay),
                cancellationToken);
            return NoContent();
        });

    [HttpDelete("cards/{cardId:guid}")]
    public Task<IActionResult> DeleteCard(Guid cardId, CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            await financialUseCases.DeleteCreditCardAsync(currentUser.UserId, cardId, cancellationToken);
            return NoContent();
        });

    [HttpGet("transactions")]
    public Task<IActionResult> GetTransactions(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var transactions = await financialUseCases.ListTransactionsAsync(
                currentUser.UserId, from, to, cancellationToken);
            return Ok(transactions.Select(ToDto).ToArray());
        });

    [HttpPost("transactions")]
    public Task<IActionResult> CreateTransaction(
        SaveFinancialTransactionRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var transactions = await financialUseCases.CreateTransactionsAsync(
                currentUser.UserId, ToInput(request), cancellationToken);
            return CreatedAtAction(nameof(GetTransactions), ToDto(transactions[0]));
        });

    [HttpPut("transactions/{transactionId:guid}")]
    public Task<IActionResult> UpdateTransaction(
        Guid transactionId,
        SaveFinancialTransactionRequest request,
        [FromQuery] TransactionSeriesScope scope = TransactionSeriesScope.Single,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () =>
        {
            await financialUseCases.UpdateTransactionAsync(
                currentUser.UserId, transactionId, ToInput(request), scope, cancellationToken);
            return NoContent();
        });

    [HttpDelete("transactions/{transactionId:guid}")]
    public Task<IActionResult> DeleteTransaction(
        Guid transactionId,
        [FromQuery] TransactionSeriesScope scope = TransactionSeriesScope.Single,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () =>
        {
            await financialUseCases.DeleteTransactionAsync(
                currentUser.UserId, transactionId, scope, cancellationToken);
            return NoContent();
        });

    [HttpGet("transactions/{transactionId:guid}/shares")]
    public Task<IActionResult> GetTransactionShares(
        Guid transactionId,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var shares = await financialUseCases.ListSharesAsync(
                currentUser.UserId, transactionId, cancellationToken);
            return Ok(shares.Select(share => new TransactionShareDto(
                share.WalletspaceId, share.WalletspaceName, share.SharedAt)).ToArray());
        });

    [HttpPost("transactions/{transactionId:guid}/shares")]
    public Task<IActionResult> ShareTransaction(
        Guid transactionId,
        ShareTransactionRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            await financialUseCases.ShareTransactionAsync(
                currentUser.UserId, transactionId, request.WalletspaceId, cancellationToken);
            var share = (await financialUseCases.ListSharesAsync(
                currentUser.UserId, transactionId, cancellationToken))
                .Single(item => item.WalletspaceId == request.WalletspaceId);
            return CreatedAtAction(
                nameof(GetTransactionShares),
                new { transactionId },
                new TransactionShareDto(share.WalletspaceId, share.WalletspaceName, share.SharedAt));
        });

    [HttpDelete("transactions/{transactionId:guid}/shares/{walletspaceId:guid}")]
    public Task<IActionResult> RemoveTransactionShare(
        Guid transactionId,
        Guid walletspaceId,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            await financialUseCases.RemoveTransactionShareAsync(
                currentUser.UserId, transactionId, walletspaceId, cancellationToken);
            return NoContent();
        });

    private async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> operation)
    {
        try
        {
            return await operation();
        }
        catch (FinancialUseCaseException exception)
        {
            var status = exception.Error switch
            {
                FinancialError.NotFound => StatusCodes.Status404NotFound,
                FinancialError.Forbidden => StatusCodes.Status403Forbidden,
                FinancialError.Conflict => StatusCodes.Status409Conflict,
                FinancialError.InvalidRequest => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
            return StatusCode(status, new ProblemDetails
            {
                Title = exception.Message,
                Status = status
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    private static FinancialAccountDto ToDto(FinancialAccountView view) =>
        new(
            view.Account.Id,
            view.Account.Name,
            view.Account.Type,
            view.Account.Institution,
            view.Account.OpeningBalance,
            view.CurrentBalance);

    private static CreditCardDto ToDto(CreditCardView view) =>
        new(
            view.Card.Id,
            view.Card.Name,
            view.Card.LastFourDigits,
            view.Card.CreditLimit,
            view.Card.ClosingDay,
            view.Card.DueDay,
            view.Outstanding);

    private static FinancialTransactionDto ToDto(FinancialTransaction transaction) =>
        new(
            transaction.Id,
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

    private static FinancialTransactionInput ToInput(SaveFinancialTransactionRequest request) =>
        new(
            request.FinancialAccountId,
            request.CreditCardId,
            request.Description,
            request.Category,
            request.Amount,
            request.Type,
            request.Status,
            request.TransactionDate,
            request.DueDate,
            request.Recurrence,
            request.Occurrences,
            request.AmountIsPerInstallment);
}
