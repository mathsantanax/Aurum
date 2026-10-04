namespace Aurum.Domain.Entities.Accounts;

public sealed class TransactionWalletspaceShare : BaseEntity
{
    public Guid FinancialTransactionId { get; private set; }
    public Guid WalletspaceId { get; private set; }

    private TransactionWalletspaceShare()
    {
    }

    public TransactionWalletspaceShare(
        Guid financialTransactionId,
        Guid walletspaceId,
        Guid sharedBy)
    {
        if (financialTransactionId == Guid.Empty)
            throw new ArgumentException("A transação é obrigatória.", nameof(financialTransactionId));
        if (walletspaceId == Guid.Empty)
            throw new ArgumentException("O Walletspace é obrigatório.", nameof(walletspaceId));

        FinancialTransactionId = financialTransactionId;
        WalletspaceId = walletspaceId;
        SetCreatedInfo(sharedBy);
    }
}
