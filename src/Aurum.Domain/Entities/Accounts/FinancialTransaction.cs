using Aurum.Domain.Enums;

namespace Aurum.Domain.Entities.Accounts;

public class FinancialTransaction : BaseEntity
{
    public Guid OwnerUserId { get; private set; }
    public Guid? FinancialAccountId { get; private set; }
    public Guid? CreditCardId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Category { get; private set; }
    public decimal Amount { get; private set; }
    public FinancialTransactionType Type { get; private set; }
    public FinancialTransactionStatus Status { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public FinancialTransactionRecurrence Recurrence { get; private set; }
    public Guid? SeriesId { get; private set; }
    public int? InstallmentNumber { get; private set; }
    public int? InstallmentCount { get; private set; }

    private FinancialTransaction()
    {
    }

    public FinancialTransaction(
        Guid ownerUserId,
        Guid? financialAccountId,
        Guid? creditCardId,
        string description,
        string? category,
        decimal amount,
        FinancialTransactionType type,
        FinancialTransactionStatus status,
        DateOnly transactionDate,
        DateOnly? dueDate,
        Guid createdBy)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("O proprietário é obrigatório.", nameof(ownerUserId));

        OwnerUserId = ownerUserId;
        SetDetails(
            financialAccountId,
            creditCardId,
            description,
            category,
            amount,
            type,
            status,
            transactionDate,
            dueDate);
        SetCreatedInfo(createdBy);
    }

    public void MarkAsSeries(
        FinancialTransactionRecurrence recurrence,
        Guid seriesId,
        int number,
        int count)
    {
        if (recurrence == FinancialTransactionRecurrence.None || !Enum.IsDefined(recurrence))
            throw new ArgumentException("Recorrência inválida.", nameof(recurrence));
        if (seriesId == Guid.Empty)
            throw new ArgumentException("A série é obrigatória.", nameof(seriesId));
        if (count < 2 || count > MaxOccurrences || number < 1 || number > count)
            throw new ArgumentOutOfRangeException(nameof(count), $"A recorrência deve ter entre 2 e {MaxOccurrences} ocorrências.");

        Recurrence = recurrence;
        SeriesId = seriesId;
        InstallmentNumber = number;
        InstallmentCount = count;
    }

    public const int MaxOccurrences = 120;

    public void SetDetails(
        Guid? financialAccountId,
        Guid? creditCardId,
        string description,
        string? category,
        decimal amount,
        FinancialTransactionType type,
        FinancialTransactionStatus status,
        DateOnly transactionDate,
        DateOnly? dueDate,
        Guid? updatedBy = null)
    {
        if (financialAccountId is null && creditCardId is null)
            throw new ArgumentException("Selecione uma conta ou um cartão.");
        if (financialAccountId is not null && creditCardId is not null)
            throw new ArgumentException("Um lançamento deve pertencer a uma conta ou a um cartão, não aos dois.");
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 200)
            throw new ArgumentException("A descrição deve ter entre 1 e 200 caracteres.", nameof(description));
        if (amount <= 0 || amount > 1_000_000_000_000m)
            throw new ArgumentOutOfRangeException(nameof(amount), "O valor deve ser maior que zero.");
        if (!Enum.IsDefined(type) || !Enum.IsDefined(status))
            throw new ArgumentOutOfRangeException(nameof(type));

        FinancialAccountId = financialAccountId;
        CreditCardId = creditCardId;
        Description = description.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        Amount = amount;
        Type = type;
        Status = status;
        TransactionDate = transactionDate;
        DueDate = dueDate;
        if (updatedBy.HasValue)
            MarkAsUpdated(updatedBy.Value);
    }
}
