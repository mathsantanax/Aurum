using Aurum.Application.Financial;
using Aurum.Domain.Entities.Accounts;
using Aurum.Domain.Enums;
using Aurum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Infrastructure.Repositories;

public sealed class FinancialRepository(AurumDbContext db) : IFinancialRepository
{
    public async Task<IReadOnlyList<FinancialAccount>> ListAccountsAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken) =>
        await db.FinancialAccounts.AsNoTracking()
            .Where(account => account.OwnerUserId == ownerUserId)
            .OrderBy(account => account.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetAccountActivityAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken) =>
        await db.FinancialTransactions.AsNoTracking()
            .Where(transaction =>
                transaction.OwnerUserId == ownerUserId &&
                transaction.FinancialAccountId.HasValue &&
                transaction.Status == FinancialTransactionStatus.Paid)
            .GroupBy(transaction => transaction.FinancialAccountId!.Value)
            .Select(group => new
            {
                AccountId = group.Key,
                Amount = group.Sum(transaction =>
                    transaction.Type == FinancialTransactionType.Income
                        ? transaction.Amount
                        : -transaction.Amount)
            })
            .ToDictionaryAsync(item => item.AccountId, item => item.Amount, cancellationToken);

    public Task<FinancialAccount?> FindAccountAsync(
        Guid ownerUserId,
        Guid accountId,
        CancellationToken cancellationToken) =>
        db.FinancialAccounts.SingleOrDefaultAsync(
            account => account.Id == accountId && account.OwnerUserId == ownerUserId,
            cancellationToken);

    public Task<bool> HasAccountTransactionsAsync(Guid accountId, CancellationToken cancellationToken) =>
        db.FinancialTransactions.AnyAsync(
            transaction => transaction.FinancialAccountId == accountId,
            cancellationToken);

    public void AddAccount(FinancialAccount account) => db.FinancialAccounts.Add(account);
    public void RemoveAccount(FinancialAccount account) => db.FinancialAccounts.Remove(account);

    public async Task<IReadOnlyList<CreditCard>> ListCreditCardsAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken) =>
        await db.CreditCards.AsNoTracking()
            .Where(card => card.OwnerUserId == ownerUserId)
            .OrderBy(card => card.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetCardOutstandingAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken) =>
        await db.FinancialTransactions.AsNoTracking()
            .Where(transaction =>
                transaction.OwnerUserId == ownerUserId &&
                transaction.CreditCardId.HasValue &&
                transaction.Type == FinancialTransactionType.Expense &&
                transaction.Status == FinancialTransactionStatus.Pending)
            .GroupBy(transaction => transaction.CreditCardId!.Value)
            .Select(group => new { CardId = group.Key, Amount = group.Sum(transaction => transaction.Amount) })
            .ToDictionaryAsync(item => item.CardId, item => item.Amount, cancellationToken);

    public Task<CreditCard?> FindCreditCardAsync(
        Guid ownerUserId,
        Guid cardId,
        CancellationToken cancellationToken) =>
        db.CreditCards.SingleOrDefaultAsync(
            card => card.Id == cardId && card.OwnerUserId == ownerUserId,
            cancellationToken);

    public Task<bool> HasCardTransactionsAsync(Guid cardId, CancellationToken cancellationToken) =>
        db.FinancialTransactions.AnyAsync(
            transaction => transaction.CreditCardId == cardId,
            cancellationToken);

    public void AddCreditCard(CreditCard card) => db.CreditCards.Add(card);
    public void RemoveCreditCard(CreditCard card) => db.CreditCards.Remove(card);

    public async Task<IReadOnlyList<FinancialTransaction>> ListTransactionsAsync(
        Guid ownerUserId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        var query = db.FinancialTransactions.AsNoTracking()
            .Where(transaction => transaction.OwnerUserId == ownerUserId);
        if (from.HasValue)
            query = query.Where(transaction => transaction.TransactionDate >= from.Value);
        if (to.HasValue)
            query = query.Where(transaction => transaction.TransactionDate <= to.Value);
        return await query.OrderByDescending(transaction => transaction.TransactionDate)
            .ThenBy(transaction => transaction.Description)
            .ToListAsync(cancellationToken);
    }

    public Task<FinancialTransaction?> FindTransactionAsync(
        Guid ownerUserId,
        Guid transactionId,
        CancellationToken cancellationToken) =>
        db.FinancialTransactions.SingleOrDefaultAsync(
            transaction => transaction.Id == transactionId && transaction.OwnerUserId == ownerUserId,
            cancellationToken);

    public async Task<IReadOnlyList<FinancialTransaction>> FindSeriesAsync(
        Guid ownerUserId,
        Guid seriesId,
        DateOnly? from,
        CancellationToken cancellationToken)
    {
        var query = db.FinancialTransactions.Where(transaction =>
            transaction.OwnerUserId == ownerUserId && transaction.SeriesId == seriesId);
        if (from.HasValue)
            query = query.Where(transaction => transaction.TransactionDate >= from.Value);
        return await query.OrderBy(transaction => transaction.TransactionDate).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SharedTransaction>> ListSharedTransactionsAsync(
        Guid memberUserId,
        Guid walletspaceId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        var query =
            from share in db.TransactionWalletspaceShares.AsNoTracking()
            join transaction in db.FinancialTransactions.AsNoTracking()
                on share.FinancialTransactionId equals transaction.Id
            join owner in db.Users.AsNoTracking() on transaction.OwnerUserId equals owner.Id
            join member in db.WalletspaceMembers.AsNoTracking()
                on share.WalletspaceId equals member.WalletspaceId
            where share.WalletspaceId == walletspaceId && member.UserId == memberUserId
            select new { Transaction = transaction, OwnerDisplayName = owner.FullName };
        if (from.HasValue)
            query = query.Where(item => item.Transaction.TransactionDate >= from.Value);
        if (to.HasValue)
            query = query.Where(item => item.Transaction.TransactionDate <= to.Value);

        var rows = await query.OrderByDescending(item => item.Transaction.TransactionDate)
            .ThenBy(item => item.Transaction.Description)
            .ToListAsync(cancellationToken);
        return rows.Select(item => new SharedTransaction(
            item.Transaction,
            item.OwnerDisplayName)).ToArray();
    }

    public Task<bool> IsWalletspaceMemberAsync(
        Guid userId,
        Guid walletspaceId,
        CancellationToken cancellationToken) =>
        db.WalletspaceMembers.AsNoTracking().AnyAsync(
            member => member.UserId == userId && member.WalletspaceId == walletspaceId,
            cancellationToken);

    public async Task<IReadOnlyList<SharedWalletspace>> ListSharesAsync(
        Guid ownerUserId,
        Guid transactionId,
        CancellationToken cancellationToken) =>
        await (
            from share in db.TransactionWalletspaceShares.AsNoTracking()
            join transaction in db.FinancialTransactions.AsNoTracking()
                on share.FinancialTransactionId equals transaction.Id
            join walletspace in db.Walletspaces.AsNoTracking()
                on share.WalletspaceId equals walletspace.Id
            where transaction.OwnerUserId == ownerUserId && transaction.Id == transactionId
            orderby walletspace.Name
            select new SharedWalletspace(walletspace.Id, walletspace.Name, share.CreatedAt))
            .ToListAsync(cancellationToken);

    public Task<TransactionWalletspaceShare?> FindShareAsync(
        Guid ownerUserId,
        Guid transactionId,
        Guid walletspaceId,
        CancellationToken cancellationToken) =>
        (
            from share in db.TransactionWalletspaceShares
            join transaction in db.FinancialTransactions
                on share.FinancialTransactionId equals transaction.Id
            where transaction.OwnerUserId == ownerUserId &&
                  transaction.Id == transactionId &&
                  share.WalletspaceId == walletspaceId
            select share)
            .SingleOrDefaultAsync(cancellationToken);

    public Task<bool> HasShareAsync(
        Guid transactionId,
        Guid walletspaceId,
        CancellationToken cancellationToken) =>
        db.TransactionWalletspaceShares.AnyAsync(
            share => share.FinancialTransactionId == transactionId && share.WalletspaceId == walletspaceId,
            cancellationToken);

    public void AddTransactions(IReadOnlyCollection<FinancialTransaction> transactions) =>
        db.FinancialTransactions.AddRange(transactions);

    public void RemoveTransactions(IReadOnlyCollection<FinancialTransaction> transactions) =>
        db.FinancialTransactions.RemoveRange(transactions);

    public void AddShare(TransactionWalletspaceShare share) => db.TransactionWalletspaceShares.Add(share);
    public void RemoveShare(TransactionWalletspaceShare share) => db.TransactionWalletspaceShares.Remove(share);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
