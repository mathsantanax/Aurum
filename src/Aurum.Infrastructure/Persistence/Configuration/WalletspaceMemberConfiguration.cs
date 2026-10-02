using Aurum.Domain.Entities.Workspace;
using Aurum.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Infrastructure.Persistence.Configuration
{
    public class WalletspaceMemberConfiguration
        : IEntityTypeConfiguration<WalletspaceMember>
    {
        public void Configure(EntityTypeBuilder<WalletspaceMember> builder)
        {
            builder.ToTable("WalletspaceMembers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Role)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.JoinedAt)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.Property(x => x.UpdatedBy)
                .IsRequired(false);

            builder.HasOne<AurumUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new
            {
                x.WalletspaceId,
                x.UserId
            })
            .IsUnique();
        }
    }
}
