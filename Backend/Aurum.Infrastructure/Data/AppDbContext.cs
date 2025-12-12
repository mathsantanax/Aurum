using Microsoft.EntityFrameworkCore;
using Aurum.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;

namespace Aurum.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // Define DbSets for your entities here
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<CreditCardExpense> CreditCardExpenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configurações adicionais de mapeamento podem ser feitas aqui

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configuração da entidade Wallet
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(e => e.Id); // Chave primária
                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired(); // Define o tamanho máximo e obrigatoriedade para o nome da carteira
                entity.Property(e => e.CreatedAt)
                      .IsRequired(); // Define a obrigatoriedade para a data de criação

                entity.Property("_balance")
                    .HasColumnName("Balance")
                    .IsRequired(); // Mapeia o campo privado _balance para a coluna Balance
                entity.HasIndex(e => e.OwnerGuid).IsUnique(); // Índice na coluna OwnerGuid
                entity.HasMany(entity => entity.Transactions)
                      .WithOne()
                      .HasForeignKey("WalletId")
                      .OnDelete(DeleteBehavior.Cascade); // Configura o relacionamento com Transaction
                entity.HasMany(entity => entity.CreditCards)
                      .WithOne()
                      .HasForeignKey("WalletId")
                      .OnDelete(DeleteBehavior.Cascade); // Configura o relacionamento com CreditCard
                entity.HasMany(entity => entity.SharedWalletMemberships)
                      .WithOne()
                      .HasForeignKey("WalletId")
                      .OnDelete(DeleteBehavior.NoAction); // Configura o relacionamento com SharedWalletMembership
            });

            // Configuração da entidade CreditCard
            modelBuilder.Entity<CreditCard>(entity =>
            {
                entity.HasKey(e => e.Id); // Chave primária
                entity.HasIndex(e => e.UserId); // Índice na coluna UserId
                entity.HasIndex(e => e.WalletId); // Índice na coluna WalletId

                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired(); // Define o tamanho máximo para o nome do cartão
                entity.Property(e => e.CreditLimit)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired(); // Define o tipo decimal para o limite de crédito
                entity.Property(e => e.CurrentBillAmount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired(); // Define o tipo decimal para o valor atual da fatura

                entity.HasMany(entity => entity.Transactions)
                      .WithOne()
                      .HasForeignKey(t => t.CreditCardGuid)
                      .OnDelete(DeleteBehavior.Cascade); // Configura o relacionamento com Transaction
            });

            // Configuração da entidade Category
            modelBuilder.Entity<Category>(entity => 
            {
                entity.HasKey(e => e.Id); // Chave primária
                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired(); // Define o tamanho máximo e obrigatoriedade para o nome da categoria
                entity.Property(e => e.Description)
                      .HasMaxLength(250); // Define o tamanho máximo para a descrição da categoria
                entity.Property(e => e.Flow)
                      .HasConversion<string>() // Mapeamento do enum para string
                      .IsRequired(); // Define a obrigatoriedade para o campo IsIncome
                entity.Property(e => e.CreatedAt)
                      .IsRequired(); // Define a obrigatoriedade para a data de criação
                entity.HasIndex(e => e.UserGuid); // Índice na coluna UserGuid
            });


            // Configuração da entidade Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id); // Chave primária
                entity.HasIndex(e => e.CategoryId); // Índice na coluna CategoryId
                entity.HasIndex(e => e.CreditCardGuid); // Índice na coluna CreditCardId
                entity.HasIndex(e => e.WalletId); // Índice na coluna WalletId
                entity.HasIndex(e => e.UserGuid); // Índice na coluna UserId
                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired(); // Define o tipo decimal para o valor da transação
                entity.Property(e => e.Description)
                      .HasMaxLength(250); // Define o tamanho máximo para a descrição da transação
                entity.Property(e => e.CreatedDate)
                      .IsRequired(); // Define a obrigatoriedade para a data da transação
                // Mapeamento de Enums
                entity.Property(t => t.Type).HasConversion<string>();
                entity.Property(t => t.PaymentMethod).HasConversion<string>();

                // Mapeia o campo de apoio para a coleção de Parcelas
                entity.Metadata.FindNavigation(nameof(Transaction.InstallmentNumber))?
                      .SetPropertyAccessMode(PropertyAccessMode.Field);
            });

            // Configuração da entidade CreditCardExpenses
            modelBuilder.Entity<CreditCardExpense>(entity =>
            {
                entity.HasKey(e => e.Id); // Chave primária
                entity.HasIndex(e => e.CreditCardGuid); // Índice na coluna CreditCardId
                entity.HasIndex(e => e.WalletId); // Índice na coluna WalletId
                entity.HasIndex(e => e.TransactiontGuid); // Índice na coluna TransactiontGuid
                entity.HasIndex(e => e.CategoryId); // Índice na coluna CategoryId
                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired(); // Define o tipo decimal para o valor da parcela
                entity.Property(e => e.InstallmentNumber)
                        .IsRequired(); // Define a obrigatoriedade para o número da parcela

                entity.HasOne<Transaction>()
                      .WithMany()
                      .HasForeignKey(e => e.TransactiontGuid)
                      .OnDelete(DeleteBehavior.Restrict); // Não deleta a transação mãe se as parcelas existirem

                // Mapeamento de Enums
                entity.Property(e => e.Type).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.PaymentMethod).HasConversion<string>();
            });

            // Configuração da entidade Invite
            modelBuilder.Entity<Invite>(entity =>
            {
                entity.HasKey(e => e.Id); // Chave primária
                entity.HasIndex(e => e.InvitedUserId); // Índice na coluna InviterUserGuid
                entity.HasIndex(e => e.WalletId); // Índice na coluna WalletId
                entity.HasIndex(e => e.SendUserId); // Índice na coluna SendUserGuid

                entity.Property(e => e.Status)
                        .HasConversion<string>() // Mapeamento do enum para string
                        .IsRequired(); // Define a obrigatoriedade para o status do convite
                entity.Property(e => e.SendAt)
                        .IsRequired(); // Define a obrigatoriedade para a data de envio do convite
                entity.Property(entity => entity.ExpireAt)
                        .IsRequired(); // Define a obrigatoriedade para a data de expiração do convite

            });

            // Configuração da entidade SharedWalletMembership
            modelBuilder.Entity<SharedWalletMembership>(entity =>
            {
                entity.HasKey(e => e.Id); // Chave primária
                entity.HasIndex(e => e.WalletId); // Índice na coluna WalletId
                entity.HasIndex(e => e.UserGuid); // Índice na coluna UserGuid
                entity.Property(e => e.JoinedAt)
                        .IsRequired(); // Define a obrigatoriedade para a data de ingresso na carteira compartilhada
                entity.Property(e => e.Role)
                        .HasConversion<string>()
                        .IsRequired(); // Define a obrigatoriedade para o papel do membro na carteira compartilhada

                entity.OwnsOne(m => m.ContribuitionRule, rule =>
                {
                    rule.Property(r => r.Type)
                        .HasConversion<string>()
                        .HasColumnName("ContribuitionType")
                        .HasMaxLength(20);

                    rule.Property(r => r.Value)
                        .HasColumnName("ContribuitionValue")
                        .HasColumnType("decimal(18,2)");
                });
            });
        }
    }
}
