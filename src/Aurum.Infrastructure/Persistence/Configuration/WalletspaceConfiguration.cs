using Aurum.Domain.Entities.Workspace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Infrastructure.Persistence.Configuration
{
    public class WalletspaceConfiguration : IEntityTypeConfiguration<Walletspace>
    {
        public void Configure(EntityTypeBuilder<Walletspace> builder)
        {
            builder.ToTable("Walletspaces");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.Property(x => x.UpdatedBy)
                .IsRequired(false);

            builder.HasMany(x => x.Members)
                .WithOne(x => x.Walletspace)
                .HasForeignKey(x => x.WalletspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
