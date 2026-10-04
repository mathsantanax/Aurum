using Aurum.Domain.Entities.Accounts;

namespace Aurum.Application.Financial;

public sealed record SharedTransaction(
    FinancialTransaction Transaction,
    string? OwnerDisplayName);

public sealed record SharedWalletspace(
    Guid WalletspaceId,
    string WalletspaceName,
    DateTime SharedAt);

public interface IFinancialRepository
{
    Task<IReadOnlyList<FinancialAccount>> ListAccountsAsync(Guid ownerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, decimal>> GetAccountActivityAsync(Guid ownerUserId, CancellationToken cancellationToken);
    Task<FinancialAccount?> FindAccountAsync(Guid ownerUserId, Guid accountId, CancellationToken cancellationToken);
    Task<bool> HasAccountTransactionsAsync(Guid accountId, CancellationToken cancellationToken);
    void AddAccount(FinancialAccount account);
    void RemoveAccount(FinancialAccount account);

    Task<IReadOnlyList<CreditCard>> ListCreditCardsAsync(Guid ownerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, decimal>> GetCardOutstandingAsync(Guid ownerUserId, CancellationToken cancellationToken);
    Task<CreditCard?> FindCreditCardAsync(Guid ownerUserId, Guid cardId, CancellationToken cancellationToken);
    Task<bool> HasCardTransactionsAsync(Guid cardId, CancellationToken cancellationToken);
    void AddCreditCard(CreditCard card);
    void RemoveCreditCard(CreditCard card);

    Task<IReadOnlyList<FinancialTransaction>> ListTransactionsAsync(
        Guid ownerUserId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken);
    Task<FinancialTransaction?> FindTransactionAsync(
        Guid ownerUserId,
        Guid transactionId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<FinancialTransaction>> FindSeriesAsync(
        Guid ownerUserId,
        Guid seriesId,
        DateOnly? from,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<SharedTransaction>> ListSharedTransactionsAsync(
        Guid memberUserId,
        Guid walletspaceId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken);
    Task<bool> IsWalletspaceMemberAsync(Guid userId, Guid walletspaceId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SharedWalletspace>> ListSharesAsync(
        Guid ownerUserId,
        Guid transactionId,
        CancellationToken cancellationToken);
    Task<TransactionWalletspaceShare?> FindShareAsync(
        Guid ownerUserId,
        Guid transactionId,
        Guid walletspaceId,
        CancellationToken cancellationToken);
    Task<bool> HasShareAsync(Guid transactionId, Guid walletspaceId, CancellationToken cancellationToken);
    void AddTransactions(IReadOnlyCollection<FinancialTransaction> transactions);
    void RemoveTransactions(IReadOnlyCollection<FinancialTransaction> transactions);
    void AddShare(TransactionWalletspaceShare share);
    void RemoveShare(TransactionWalletspaceShare share);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
