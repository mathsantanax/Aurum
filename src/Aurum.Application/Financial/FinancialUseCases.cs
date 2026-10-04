using Aurum.Domain.Entities.Accounts;
using Aurum.Domain.Enums;
using Aurum.Domain.Services;

namespace Aurum.Application.Financial;

public enum TransactionSeriesScope
{
    Single = 0,
    Future = 1,
    All = 2
}

public enum FinancialError
{
    NotFound,
    Forbidden,
    Conflict,
    InvalidRequest
}

public sealed class FinancialUseCaseException(FinancialError error, string message) : Exception(message)
{
    public FinancialError Error { get; } = error;
}

public sealed record FinancialAccountInput(
    string Name,
    FinancialAccountType Type,
    decimal OpeningBalance,
    string? Institution);

public sealed record CreditCardInput(
    string Name,
    string LastFourDigits,
    decimal CreditLimit,
    int ClosingDay,
    int DueDay);

public sealed record FinancialTransactionInput(
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
    int? Occurrences,
    bool AmountIsPerInstallment);

public sealed record FinancialAccountView(FinancialAccount Account, decimal CurrentBalance);
public sealed record CreditCardView(CreditCard Card, decimal Outstanding);
public sealed record CategoryTotal(string Category, FinancialTransactionType Type, decimal Total);

public sealed record FinancialSummary(
    DateOnly From,
    DateOnly To,
    decimal Income,
    decimal Expense,
    decimal Net,
    int PendingCount,
    IReadOnlyList<CategoryTotal> Categories);

public sealed record PersonalDashboard(
    decimal AccountBalance,
    decimal IncomeThisMonth,
    decimal ExpenseThisMonth);

public interface IFinancialUseCases
{
    Task<IReadOnlyList<FinancialAccountView>> ListAccountsAsync(Guid userId, CancellationToken cancellationToken);
    Task<FinancialAccountView> CreateAccountAsync(Guid userId, FinancialAccountInput input, CancellationToken cancellationToken);
    Task UpdateAccountAsync(Guid userId, Guid accountId, FinancialAccountInput input, CancellationToken cancellationToken);
    Task DeleteAccountAsync(Guid userId, Guid accountId, CancellationToken cancellationToken);

    Task<IReadOnlyList<CreditCardView>> ListCreditCardsAsync(Guid userId, CancellationToken cancellationToken);
    Task<CreditCardView> CreateCreditCardAsync(Guid userId, CreditCardInput input, CancellationToken cancellationToken);
    Task UpdateCreditCardAsync(Guid userId, Guid cardId, CreditCardInput input, CancellationToken cancellationToken);
    Task DeleteCreditCardAsync(Guid userId, Guid cardId, CancellationToken cancellationToken);

    Task<IReadOnlyList<FinancialTransaction>> ListTransactionsAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<FinancialTransaction>> CreateTransactionsAsync(
        Guid userId,
        FinancialTransactionInput input,
        CancellationToken cancellationToken);
    Task UpdateTransactionAsync(
        Guid userId,
        Guid transactionId,
        FinancialTransactionInput input,
        TransactionSeriesScope scope,
        CancellationToken cancellationToken);
    Task DeleteTransactionAsync(
        Guid userId,
        Guid transactionId,
        TransactionSeriesScope scope,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<SharedTransaction>> ListWalletspaceTransactionsAsync(
        Guid userId,
        Guid walletspaceId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken);
    Task<FinancialSummary> GetWalletspaceSummaryAsync(
        Guid userId,
        Guid walletspaceId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<SharedWalletspace>> ListSharesAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken);
    Task ShareTransactionAsync(
        Guid userId,
        Guid transactionId,
        Guid walletspaceId,
        CancellationToken cancellationToken);
    Task RemoveTransactionShareAsync(
        Guid userId,
        Guid transactionId,
        Guid walletspaceId,
        CancellationToken cancellationToken);
    Task<PersonalDashboard> GetPersonalDashboardAsync(Guid userId, CancellationToken cancellationToken);
}

public sealed class FinancialUseCases(IFinancialRepository repository) : IFinancialUseCases
{
    public async Task<IReadOnlyList<FinancialAccountView>> ListAccountsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var accounts = await repository.ListAccountsAsync(userId, cancellationToken);
        var activity = await repository.GetAccountActivityAsync(userId, cancellationToken);
        return accounts.Select(account =>
            new FinancialAccountView(account, account.OpeningBalance + activity.GetValueOrDefault(account.Id)))
            .ToArray();
    }

    public async Task<FinancialAccountView> CreateAccountAsync(
        Guid userId,
        FinancialAccountInput input,
        CancellationToken cancellationToken)
    {
        EnsureUserId(userId);
        var account = new FinancialAccount(
            userId, input.Name, input.Type, input.OpeningBalance, input.Institution, userId);
        repository.AddAccount(account);
        await repository.SaveChangesAsync(cancellationToken);
        return new FinancialAccountView(account, account.OpeningBalance);
    }

    public async Task UpdateAccountAsync(
        Guid userId,
        Guid accountId,
        FinancialAccountInput input,
        CancellationToken cancellationToken)
    {
        var account = await repository.FindAccountAsync(userId, accountId, cancellationToken)
            ?? throw NotFound("A conta não foi encontrada.");
        account.SetDetails(input.Name, input.Type, input.OpeningBalance, input.Institution, userId);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAccountAsync(Guid userId, Guid accountId, CancellationToken cancellationToken)
    {
        var account = await repository.FindAccountAsync(userId, accountId, cancellationToken)
            ?? throw NotFound("A conta não foi encontrada.");
        if (await repository.HasAccountTransactionsAsync(accountId, cancellationToken))
            throw Conflict("A conta possui lançamentos vinculados e não pode ser removida.");

        repository.RemoveAccount(account);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CreditCardView>> ListCreditCardsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var cards = await repository.ListCreditCardsAsync(userId, cancellationToken);
        var outstanding = await repository.GetCardOutstandingAsync(userId, cancellationToken);
        return cards.Select(card =>
            new CreditCardView(card, outstanding.GetValueOrDefault(card.Id))).ToArray();
    }

    public async Task<CreditCardView> CreateCreditCardAsync(
        Guid userId,
        CreditCardInput input,
        CancellationToken cancellationToken)
    {
        EnsureUserId(userId);
        var card = new CreditCard(
            userId, input.Name, input.LastFourDigits, input.CreditLimit,
            input.ClosingDay, input.DueDay, userId);
        repository.AddCreditCard(card);
        await repository.SaveChangesAsync(cancellationToken);
        return new CreditCardView(card, 0);
    }

    public async Task UpdateCreditCardAsync(
        Guid userId,
        Guid cardId,
        CreditCardInput input,
        CancellationToken cancellationToken)
    {
        var card = await repository.FindCreditCardAsync(userId, cardId, cancellationToken)
            ?? throw NotFound("O cartão não foi encontrado.");
        card.SetDetails(
            input.Name, input.LastFourDigits, input.CreditLimit,
            input.ClosingDay, input.DueDay, userId);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCreditCardAsync(Guid userId, Guid cardId, CancellationToken cancellationToken)
    {
        var card = await repository.FindCreditCardAsync(userId, cardId, cancellationToken)
            ?? throw NotFound("O cartão não foi encontrado.");
        if (await repository.HasCardTransactionsAsync(cardId, cancellationToken))
            throw Conflict("O cartão possui lançamentos vinculados e não pode ser removido.");

        repository.RemoveCreditCard(card);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<FinancialTransaction>> ListTransactionsAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        ValidateDateRange(from, to);
        return repository.ListTransactionsAsync(userId, from, to, cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialTransaction>> CreateTransactionsAsync(
        Guid userId,
        FinancialTransactionInput input,
        CancellationToken cancellationToken)
    {
        EnsureUserId(userId);
        var card = await ValidateResourcesAsync(userId, input, cancellationToken);
        if (!Enum.IsDefined(input.Recurrence))
            throw Invalid("Recorrência inválida.");
        if (input.Recurrence != FinancialTransactionRecurrence.None && input.Occurrences is null)
            throw Invalid("Informe a quantidade de meses/parcelas.");

        var plan = TransactionScheduler.Plan(
            input.Recurrence,
            input.Occurrences ?? 1,
            input.Amount,
            input.AmountIsPerInstallment,
            input.TransactionDate,
            input.DueDate,
            card?.ClosingDay,
            card?.DueDay);
        var seriesId = Guid.NewGuid();
        var created = plan.Select(occurrence =>
        {
            var transaction = new FinancialTransaction(
                userId,
                input.FinancialAccountId,
                input.CreditCardId,
                input.Description,
                input.Category,
                occurrence.Amount,
                input.Type,
                occurrence.Number == 1 ? input.Status : FinancialTransactionStatus.Pending,
                occurrence.TransactionDate,
                occurrence.DueDate,
                userId);
            if (input.Recurrence != FinancialTransactionRecurrence.None)
                transaction.MarkAsSeries(input.Recurrence, seriesId, occurrence.Number, plan.Count);
            return transaction;
        }).ToArray();

        repository.AddTransactions(created);
        await repository.SaveChangesAsync(cancellationToken);
        return created;
    }

    public async Task UpdateTransactionAsync(
        Guid userId,
        Guid transactionId,
        FinancialTransactionInput input,
        TransactionSeriesScope scope,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(scope))
            throw Invalid("Escopo da série inválido.");
        var transaction = await repository.FindTransactionAsync(userId, transactionId, cancellationToken)
            ?? throw NotFound("O lançamento não foi encontrado.");
        await ValidateResourcesAsync(userId, input, cancellationToken);

        if (scope != TransactionSeriesScope.Single && transaction.SeriesId.HasValue)
        {
            var followers = await repository.FindSeriesAsync(
                userId,
                transaction.SeriesId.Value,
                scope == TransactionSeriesScope.Future ? transaction.TransactionDate : null,
                cancellationToken);
            foreach (var follower in followers.Where(item => item.Id != transaction.Id))
                follower.SetDetails(
                    input.FinancialAccountId,
                    input.CreditCardId,
                    input.Description,
                    input.Category,
                    input.Amount,
                    input.Type,
                    follower.Status,
                    follower.TransactionDate,
                    follower.DueDate,
                    userId);
        }

        transaction.SetDetails(
            input.FinancialAccountId,
            input.CreditCardId,
            input.Description,
            input.Category,
            input.Amount,
            input.Type,
            input.Status,
            input.TransactionDate,
            input.DueDate,
            userId);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTransactionAsync(
        Guid userId,
        Guid transactionId,
        TransactionSeriesScope scope,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(scope))
            throw Invalid("Escopo da série inválido.");
        var transaction = await repository.FindTransactionAsync(userId, transactionId, cancellationToken)
            ?? throw NotFound("O lançamento não foi encontrado.");
        var deleting = scope == TransactionSeriesScope.Single || !transaction.SeriesId.HasValue
            ? [transaction]
            : await repository.FindSeriesAsync(
                userId,
                transaction.SeriesId.Value,
                scope == TransactionSeriesScope.Future ? transaction.TransactionDate : null,
                cancellationToken);
        repository.RemoveTransactions(deleting);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SharedTransaction>> ListWalletspaceTransactionsAsync(
        Guid userId,
        Guid walletspaceId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        ValidateDateRange(from, to);
        if (!await repository.IsWalletspaceMemberAsync(userId, walletspaceId, cancellationToken))
            throw NotFound("O Walletspace não foi encontrado.");
        return await repository.ListSharedTransactionsAsync(userId, walletspaceId, from, to, cancellationToken);
    }

    public async Task<FinancialSummary> GetWalletspaceSummaryAsync(
        Guid userId,
        Guid walletspaceId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        ValidateDateRange(from, to);
        if (to.DayNumber - from.DayNumber > 366)
            throw Invalid("O período do relatório não pode exceder 12 meses.");
        var shared = await ListWalletspaceTransactionsAsync(userId, walletspaceId, from, to, cancellationToken);
        var transactions = shared.Select(item => item.Transaction).ToArray();
        var paid = transactions.Where(item => item.Status == FinancialTransactionStatus.Paid).ToArray();
        var income = paid.Where(item => item.Type == FinancialTransactionType.Income).Sum(item => item.Amount);
        var expense = paid.Where(item => item.Type == FinancialTransactionType.Expense).Sum(item => item.Amount);
        var categories = paid
            .GroupBy(item => new { Category = item.Category ?? "Sem categoria", item.Type })
            .Select(group => new CategoryTotal(group.Key.Category, group.Key.Type, group.Sum(item => item.Amount)))
            .OrderByDescending(item => item.Total)
            .ToArray();

        return new FinancialSummary(
            from,
            to,
            income,
            expense,
            income - expense,
            transactions.Count(item => item.Status == FinancialTransactionStatus.Pending),
            categories);
    }

    public async Task<IReadOnlyList<SharedWalletspace>> ListSharesAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        if (await repository.FindTransactionAsync(userId, transactionId, cancellationToken) is null)
            throw NotFound("O lançamento não foi encontrado.");
        return await repository.ListSharesAsync(userId, transactionId, cancellationToken);
    }

    public async Task ShareTransactionAsync(
        Guid userId,
        Guid transactionId,
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        if (await repository.FindTransactionAsync(userId, transactionId, cancellationToken) is null)
            throw NotFound("O lançamento não foi encontrado.");
        if (!await repository.IsWalletspaceMemberAsync(userId, walletspaceId, cancellationToken))
            throw NotFound("O Walletspace não foi encontrado.");
        if (await repository.HasShareAsync(transactionId, walletspaceId, cancellationToken))
            throw Conflict("O lançamento já foi compartilhado com este Walletspace.");

        repository.AddShare(new TransactionWalletspaceShare(transactionId, walletspaceId, userId));
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveTransactionShareAsync(
        Guid userId,
        Guid transactionId,
        Guid walletspaceId,
        CancellationToken cancellationToken)
    {
        var share = await repository.FindShareAsync(userId, transactionId, walletspaceId, cancellationToken)
            ?? throw NotFound("O compartilhamento não foi encontrado.");
        repository.RemoveShare(share);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PersonalDashboard> GetPersonalDashboardAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var accounts = await ListAccountsAsync(userId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = new DateOnly(today.Year, today.Month, 1);
        var transactions = await repository.ListTransactionsAsync(userId, null, today, cancellationToken);
        var paidThisMonth = transactions
            .Where(item => item.Status == FinancialTransactionStatus.Paid && item.TransactionDate >= start)
            .ToArray();
        return new PersonalDashboard(
            accounts.Sum(item => item.CurrentBalance),
            paidThisMonth.Where(item => item.Type == FinancialTransactionType.Income).Sum(item => item.Amount),
            paidThisMonth.Where(item => item.Type == FinancialTransactionType.Expense).Sum(item => item.Amount));
    }

    private async Task<CreditCard?> ValidateResourcesAsync(
        Guid userId,
        FinancialTransactionInput input,
        CancellationToken cancellationToken)
    {
        if (input.FinancialAccountId.HasValue == input.CreditCardId.HasValue)
            throw Invalid("Selecione uma conta ou um cartão, mas não ambos.");
        if (!Enum.IsDefined(input.Type) || !Enum.IsDefined(input.Status))
            throw Invalid("Tipo ou situação de lançamento inválidos.");
        if (input.CreditCardId.HasValue && input.Type != FinancialTransactionType.Expense)
            throw Invalid("Lançamentos em cartão devem ser despesas.");

        if (input.FinancialAccountId.HasValue &&
            await repository.FindAccountAsync(userId, input.FinancialAccountId.Value, cancellationToken) is null)
            throw Invalid("A conta selecionada não pertence ao usuário.");
        if (input.CreditCardId.HasValue)
            return await repository.FindCreditCardAsync(userId, input.CreditCardId.Value, cancellationToken)
                ?? throw Invalid("O cartão selecionado não pertence ao usuário.");
        return null;
    }

    private static void ValidateDateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw Invalid("O início do período deve anteceder o fim.");
    }

    private static void EnsureUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("O usuário autenticado é obrigatório.", nameof(userId));
    }

    private static FinancialUseCaseException NotFound(string message) =>
        new(FinancialError.NotFound, message);
    private static FinancialUseCaseException Conflict(string message) =>
        new(FinancialError.Conflict, message);
    private static FinancialUseCaseException Invalid(string message) =>
        new(FinancialError.InvalidRequest, message);
}
