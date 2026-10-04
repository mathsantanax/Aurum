using Aurum.Api.Contracts.Walletspaces;
using Aurum.Application.Financial;
using Aurum.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/walletspaces/{walletspaceId:guid}")]
public sealed class WalletspaceFinanceController(
    IFinancialUseCases financialUseCases,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        Guid walletspaceId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        try
        {
            var transactions = await financialUseCases.ListWalletspaceTransactionsAsync(
                currentUser.UserId, walletspaceId, from, to, cancellationToken);
            return Ok(transactions.Select(item => new SharedFinancialTransactionDto(
                item.Transaction.Id,
                item.OwnerDisplayName,
                item.Transaction.Description,
                item.Transaction.Category,
                item.Transaction.Amount,
                item.Transaction.Type,
                item.Transaction.Status,
                item.Transaction.TransactionDate,
                item.Transaction.DueDate,
                item.Transaction.Recurrence,
                item.Transaction.SeriesId,
                item.Transaction.InstallmentNumber,
                item.Transaction.InstallmentCount)).ToArray());
        }
        catch (FinancialUseCaseException exception)
        {
            return MapError(exception);
        }
    }

    [HttpGet("reports/summary")]
    public async Task<IActionResult> GetSummary(
        Guid walletspaceId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = from ?? new DateOnly(today.Year, today.Month, 1);
        var end = to ?? today;
        try
        {
            var summary = await financialUseCases.GetWalletspaceSummaryAsync(
                currentUser.UserId, walletspaceId, start, end, cancellationToken);
            return Ok(new FinancialSummaryDto(
                walletspaceId,
                summary.From,
                summary.To,
                summary.Income,
                summary.Expense,
                summary.Net,
                summary.PendingCount,
                summary.Categories.Select(item =>
                    new CategorySummaryDto(item.Category, item.Type, item.Total)).ToArray()));
        }
        catch (FinancialUseCaseException exception)
        {
            return MapError(exception);
        }
    }

    private ObjectResult MapError(FinancialUseCaseException exception)
    {
        var status = exception.Error switch
        {
            FinancialError.NotFound => StatusCodes.Status404NotFound,
            FinancialError.Forbidden => StatusCodes.Status403Forbidden,
            FinancialError.Conflict => StatusCodes.Status409Conflict,
            FinancialError.InvalidRequest => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        return StatusCode(status, new ProblemDetails { Title = exception.Message, Status = status });
    }
}
