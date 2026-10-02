using Aurum.Domain.Entities.Workspace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Infrastructure.Persistence.Configuration;

public class FinancialTransactionConfiguration
    : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable("FinancialTransactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(100);
        builder.Property(x => x.Amount).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.TransactionDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.DueDate).HasColumnType("date");
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).IsRequired();
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.UpdatedBy);
        builder.HasOne<Walletspace>()
            .WithMany()
            .HasForeignKey(x => x.WalletspaceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<FinancialAccount>()
            .WithMany()
            .HasForeignKey(x => x.FinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CreditCard>()
            .WithMany()
            .HasForeignKey(x => x.CreditCardId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.WalletspaceId, x.TransactionDate });
        builder.HasIndex(x => x.FinancialAccountId);
        builder.HasIndex(x => x.CreditCardId);
    }
}
