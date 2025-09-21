using Aurum_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Infrastructure.Persistence
{
    public class InfraContext : DbContext
    {
        public InfraContext(DbContextOptions<InfraContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<SharedWallet> SharedWallets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Cost> Costs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ------------------------------
            // TRANSACTION (TPH - herança)
            // ------------------------------
            modelBuilder.Entity<Transaction>()
                .HasDiscriminator<string>("TransactionType")
                .HasValue<Income>("Income")
                .HasValue<Cost>("Cost");

            // ------------------------------
            // USER -> WALLETS (1:N)
            // ------------------------------
            modelBuilder.Entity<User>()
                .HasMany(u => u.Wallets)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserId);

            // ------------------------------
            // WALLET -> TRANSACTIONS (1:N)
            // ------------------------------
            modelBuilder.Entity<Wallet>()
                .HasMany(w => w.Transactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------
            // SHAREDWALLET -> TRANSACTIONS (1:N)
            // ------------------------------
            modelBuilder.Entity<SharedWallet>()
                .HasMany(sw => sw.Transactions)
                .WithOne(t => t.SharedWallet)
                .HasForeignKey(t => t.SharedWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------
            // SHAREDWALLET -> MEMBERS (N:N)
            // ------------------------------
            modelBuilder.Entity<SharedWallet>()
                .HasMany(sw => sw.Members)
                .WithMany()
                .UsingEntity(j => j.ToTable("SharedWalletMembers"));
        }
    }
}
