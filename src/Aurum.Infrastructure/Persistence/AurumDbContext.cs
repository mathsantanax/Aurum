using Aurum.Domain.Entities.Workspace;
using Aurum.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Infrastructure.Persistence
{
    public class AurumDbContext : IdentityDbContext<AurumUser, AurumRoles, Guid>
    {
        public AurumDbContext(DbContextOptions<AurumDbContext> options) : base(options)
        {
        }

        // Add Dbset propriedades do banco de dados
        public DbSet<Walletspace> Walletspaces => Set<Walletspace>();

        public DbSet<WalletspaceMember> WalletspaceMembers =>
            Set<WalletspaceMember>();

        public DbSet<FinancialAccount> FinancialAccounts => Set<FinancialAccount>();

        public DbSet<CreditCard> CreditCards => Set<CreditCard>();

        public DbSet<FinancialTransaction> FinancialTransactions =>
            Set<FinancialTransaction>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(AurumDbContext).Assembly);

            builder.Entity<AurumUser>(entity =>
            {
                entity.Property(x => x.FullName)
                    .HasMaxLength(250);
            });

        }
    }
}
