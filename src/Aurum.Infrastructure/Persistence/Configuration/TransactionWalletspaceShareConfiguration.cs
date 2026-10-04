using Aurum.Domain.Entities.Accounts;
using Aurum.Domain.Entities.Workspace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Infrastructure.Persistence.Configuration;

public sealed class TransactionWalletspaceShareConfiguration
    : IEntityTypeConfiguration<TransactionWalletspaceShare>
{
    public void Configure(EntityTypeBuilder<TransactionWalletspaceShare> builder)
    {
        builder.ToTable("TransactionWalletspaceShares");
        builder.HasKey(share => share.Id);
        builder.Property(share => share.CreatedAt).IsRequired();
        builder.Property(share => share.CreatedBy).IsRequired();
        builder.Property(share => share.UpdatedAt);
        builder.Property(share => share.UpdatedBy);
        builder.HasOne<FinancialTransaction>()
            .WithMany()
            .HasForeignKey(share => share.FinancialTransactionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Walletspace>()
            .WithMany()
            .HasForeignKey(share => share.WalletspaceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(share => new { share.FinancialTransactionId, share.WalletspaceId })
            .IsUnique();
        builder.HasIndex(share => share.WalletspaceId);
    }
}
