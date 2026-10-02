using Aurum.Domain.Entities.Workspace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Infrastructure.Persistence.Configuration;

public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
{
    public void Configure(EntityTypeBuilder<CreditCard> builder)
    {
        builder.ToTable("CreditCards");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastFourDigits).HasMaxLength(4).IsRequired();
        builder.Property(x => x.CreditLimit).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).IsRequired();
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.UpdatedBy);
        builder.HasOne<Walletspace>()
            .WithMany()
            .HasForeignKey(x => x.WalletspaceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.WalletspaceId);
    }
}
