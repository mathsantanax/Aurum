using Aurum.Domain.Enums;

namespace Aurum.Domain.Entities.Accounts;

public class FinancialAccount : BaseEntity
{
    public Guid OwnerUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public FinancialAccountType Type { get; private set; }
    public string? Institution { get; private set; }
    public decimal OpeningBalance { get; private set; }

    private FinancialAccount()
    {
    }

    public FinancialAccount(
        Guid ownerUserId,
        string name,
        FinancialAccountType type,
        decimal openingBalance,
        string? institution,
        Guid createdBy)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("O proprietário é obrigatório.", nameof(ownerUserId));

        OwnerUserId = ownerUserId;
        SetDetails(name, type, openingBalance, institution);
        SetCreatedInfo(createdBy);
    }

    public void SetDetails(
        string name,
        FinancialAccountType type,
        decimal openingBalance,
        string? institution,
        Guid? updatedBy = null)
    {
        Name = ValidateName(name);
        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));
        if (openingBalance < -1_000_000_000_000m || openingBalance > 1_000_000_000_000m)
            throw new ArgumentOutOfRangeException(nameof(openingBalance));

        Type = type;
        OpeningBalance = openingBalance;
        Institution = string.IsNullOrWhiteSpace(institution) ? null : institution.Trim();
        if (updatedBy.HasValue)
            MarkAsUpdated(updatedBy.Value);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            throw new ArgumentException("O nome deve ter entre 1 e 100 caracteres.", nameof(name));
        return name.Trim();
    }
}
