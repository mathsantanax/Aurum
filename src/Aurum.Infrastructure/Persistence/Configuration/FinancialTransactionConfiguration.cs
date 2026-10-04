using Aurum.Domain.Entities.Accounts;
using Aurum.Infrastructure.Identity;
using Aurum.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Infrastructure.Persistence.Configuration;

public class FinancialTransactionConfiguration
    : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable("FinancialTransactions", table =>
            table.HasCheckConstraint(
                "CK_FinancialTransactions_ExactlyOneResource",
                "([FinancialAccountId] IS NOT NULL AND [CreditCardId] IS NULL) OR ([FinancialAccountId] IS NULL AND [CreditCardId] IS NOT NULL)"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(100);
        builder.Property(x => x.Amount).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.TransactionDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.DueDate).HasColumnType("date");
        builder.Property(x => x.Recurrence).HasConversion<int>().HasDefaultValue(FinancialTransactionRecurrence.None).IsRequired();
        builder.Property(x => x.SeriesId);
        builder.Property(x => x.InstallmentNumber);
        builder.Property(x => x.InstallmentCount);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).IsRequired();
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.UpdatedBy);
        builder.HasOne<AurumUser>()
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinancialAccount>()
            .WithMany()
            .HasForeignKey(x => x.FinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CreditCard>()
            .WithMany()
            .HasForeignKey(x => x.CreditCardId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.OwnerUserId, x.TransactionDate });
        builder.HasIndex(x => new { x.OwnerUserId, x.TransactionDate });
        builder.HasIndex(x => x.FinancialAccountId);
        builder.HasIndex(x => x.CreditCardId);
        builder.HasIndex(x => x.SeriesId);
    }
}
