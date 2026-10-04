namespace Aurum.Domain.Entities.Accounts;

public class CreditCard : BaseEntity
{
    public Guid OwnerUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string LastFourDigits { get; private set; } = string.Empty;
    public decimal CreditLimit { get; private set; }
    public int ClosingDay { get; private set; }
    public int DueDay { get; private set; }

    private CreditCard()
    {
    }

    public CreditCard(
        Guid ownerUserId,
        string name,
        string lastFourDigits,
        decimal creditLimit,
        int closingDay,
        int dueDay,
        Guid createdBy)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("O proprietário é obrigatório.", nameof(ownerUserId));

        OwnerUserId = ownerUserId;
        SetDetails(name, lastFourDigits, creditLimit, closingDay, dueDay);
        SetCreatedInfo(createdBy);
    }

    public void SetDetails(
        string name,
        string lastFourDigits,
        decimal creditLimit,
        int closingDay,
        int dueDay,
        Guid? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            throw new ArgumentException("O nome deve ter entre 1 e 100 caracteres.", nameof(name));
        if (string.IsNullOrWhiteSpace(lastFourDigits) ||
            lastFourDigits.Length != 4 ||
            !lastFourDigits.All(char.IsDigit))
            throw new ArgumentException("Informe os quatro últimos dígitos do cartão.", nameof(lastFourDigits));
        if (creditLimit < 0 || creditLimit > 1_000_000_000_000m)
            throw new ArgumentOutOfRangeException(nameof(creditLimit));
        if (closingDay is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(closingDay));
        if (dueDay is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(dueDay));

        Name = name.Trim();
        LastFourDigits = lastFourDigits;
        CreditLimit = creditLimit;
        ClosingDay = closingDay;
        DueDay = dueDay;
        if (updatedBy.HasValue)
            MarkAsUpdated(updatedBy.Value);
    }
}
