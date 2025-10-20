using AurumApi.Models;
using AurumApi.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AurumApi.Persistence
{
    public class AurumDbContext : DbContext
    {
    public AurumDbContext(DbContextOptions<AurumDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<PrivateWallet> PrivateWallets { get; set; }
        public DbSet<SharedWallet> SharedWallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Category { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User 
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Guid);
                entity.Property(u => u.Name).IsRequired().HasMaxLength(150);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.HasMany<PrivateWallet>()
                    .WithOne(w => w.User)
                    .HasForeignKey(w => w.OwnerGuid)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Wallet
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(w => w.Guid);
                entity.Property(w => w.Name).IsRequired().HasMaxLength(50);
                entity.Property(w => w.Amount).HasConversion(typeof(decimal));
                entity.Property(w => w.CreatedAt).IsRequired();
                entity.Property(w => w.UpdatedAt).IsRequired(false);
                entity.Property(w => w.WalletType)
                    .HasConversion<string>();
                entity.HasDiscriminator<WalletType>(w => w.WalletType)
                    .HasValue<PrivateWallet>(WalletType.Private)
                    .HasValue<SharedWallet>(WalletType.Public);
            });

            // Private Wallet
            modelBuilder.Entity<PrivateWallet>(entity =>
            {
                entity.HasOne(pw => pw.User)
                    .WithMany()
                    .HasForeignKey(pw => pw.OwnerGuid)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Shared Wallet
            modelBuilder.Entity<SharedWallet>(entity =>
            {
                entity.HasMany(sw => sw.Members)
                    .WithOne(m => m.SharedWallet)
                    .HasForeignKey(m => m.WalletGuid)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Members (herda de user)
            modelBuilder.Entity<Members>()
                .HasBaseType<User>();

            // Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Guid);
                entity.Property(c => c.NameCategory)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Guid);
                entity.Property(t => t.Value)
                    .HasConversion(typeof(decimal))
                    .IsRequired();
                entity.Property(t => t.Description)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(t => t.CreatedAt)
                    .IsRequired();
                entity.HasOne(t => t.Wallet)
                    .WithMany(w => w.Transactions)
                    .HasForeignKey(t => t.WalletGuid)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Category)
                    .WithMany()
                    .HasForeignKey(t => t.CategoryGuid)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.CreatedBy)
                    .WithMany()
                    .HasForeignKey(t => t.CreatedByGuid)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            base.OnModelCreating(modelBuilder);
        }    
    }
}
