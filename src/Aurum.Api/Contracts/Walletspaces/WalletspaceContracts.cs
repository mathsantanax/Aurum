using Aurum.Domain.Enums;
using Aurum.Application.Financial;
using System.ComponentModel.DataAnnotations;

namespace Aurum.Api.Contracts.Walletspaces;

public sealed record WalletspaceDto(
    Guid Id,
    string Name,
    WalletspaceRole MyRole,
    int MemberCount,
    DateTime CreatedAt);

public sealed record CreateWalletspaceRequest(
    [Required, StringLength(100, MinimumLength = 3)] string Name);

public sealed record RenameWalletspaceRequest(
    [Required, StringLength(100, MinimumLength = 3)] string Name);

public sealed record FinancialAccountDto(
    Guid Id,
    string Name,
    FinancialAccountType Type,
    string? Institution,
    decimal OpeningBalance,
    decimal CurrentBalance);

public sealed record SaveFinancialAccountRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    FinancialAccountType Type,
    decimal OpeningBalance,
    [StringLength(100)] string? Institution);

public sealed record CreditCardDto(
    Guid Id,
    string Name,
    string LastFourDigits,
    decimal CreditLimit,
    int ClosingDay,
    int DueDay,
    decimal Outstanding);

public sealed record SaveCreditCardRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Required, RegularExpression(@"^\d{4}$")] string LastFourDigits,
    decimal CreditLimit,
    [Range(1, 31)] int ClosingDay,
    [Range(1, 31)] int DueDay);

public sealed record FinancialTransactionDto(
    Guid Id,
    Guid? FinancialAccountId,
    Guid? CreditCardId,
    string Description,
    string? Category,
    decimal Amount,
    FinancialTransactionType Type,
    FinancialTransactionStatus Status,
    DateOnly TransactionDate,
    DateOnly? DueDate,
    FinancialTransactionRecurrence Recurrence,
    Guid? SeriesId,
    int? InstallmentNumber,
    int? InstallmentCount);

public sealed record SaveFinancialTransactionRequest(
    Guid? FinancialAccountId,
    Guid? CreditCardId,
    [Required, StringLength(200, MinimumLength = 1)] string Description,
    [StringLength(100)] string? Category,
    [Range(typeof(decimal), "0.0001", "1000000000000")] decimal Amount,
    FinancialTransactionType Type,
    FinancialTransactionStatus Status,
    DateOnly TransactionDate,
    DateOnly? DueDate,
    FinancialTransactionRecurrence Recurrence = FinancialTransactionRecurrence.None,
    [Range(2, 120)] int? Occurrences = null,
    bool AmountIsPerInstallment = false);

public sealed record TransferOwnershipRequest([Required] Guid NewOwnerId);

public sealed record AddWalletspaceMemberRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    WalletspaceRole Role);

public sealed record WalletspaceMemberDto(
    Guid UserId,
    string Email,
    string? FullName,
    WalletspaceRole Role,
    DateTime JoinedAt);

public sealed record ChangeWalletspaceMemberRoleRequest(WalletspaceRole Role);

public sealed record FinancialSummaryDto(
    Guid WalletspaceId,
    DateOnly From,
    DateOnly To,
    decimal Income,
    decimal Expense,
    decimal Net,
    int PendingCount,
    IReadOnlyList<CategorySummaryDto> Categories);

public sealed record CategorySummaryDto(
    string Category,
    FinancialTransactionType Type,
    decimal Total);

public sealed record SharedFinancialTransactionDto(
    Guid Id,
    string? OwnerDisplayName,
    string Description,
    string? Category,
    decimal Amount,
    FinancialTransactionType Type,
    FinancialTransactionStatus Status,
    DateOnly TransactionDate,
    DateOnly? DueDate,
    FinancialTransactionRecurrence Recurrence,
    Guid? SeriesId,
    int? InstallmentNumber,
    int? InstallmentCount);

public sealed record TransactionShareDto(
    Guid WalletspaceId,
    string WalletspaceName,
    DateTime SharedAt);

public sealed record ShareTransactionRequest([Required] Guid WalletspaceId);

public sealed record DashboardSummaryDto(
    int WalletspaceCount,
    decimal AccountBalance,
    decimal IncomeThisMonth,
    decimal ExpenseThisMonth,
    IReadOnlyList<WalletspaceDto> Walletspaces);
