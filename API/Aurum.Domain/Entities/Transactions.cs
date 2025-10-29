using Aurum.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities
{
    public class Transactions
    {
        public Guid Guid { get; private set; }
        public decimal Value { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public WalletType WalletType { get; private set; }
        public TransactionType TransactionType { get; private set; }

        // Categoria
        public Guid CategoryGuid { get; private set; }
        public Category Category { get; private set; } = null!;
        // Carteira vinculada
        public Guid WalletGuid { get; private set; }
        public Wallet Wallet { get; private set; } = null!;
        // Usuario que criou
        public Guid CreatedByGuid { get; private set; }
        public User CreatedBy { get; private set; } = null!;



        public virtual void AddIncome(Wallet wallet, decimal value, string description, Category category)
        {
            this.Guid = Guid.NewGuid();
            this.Value = value;
            this.Description = description;
            this.CreatedAt = DateTime.Now;
            this.TransactionType = TransactionType.Income;

            this.Category = category;
            this.CategoryGuid = category.Guid;
            this.WalletGuid = wallet.Guid;
            this.Wallet = wallet;
            wallet.UpdatedAt = DateTime.Now;
            wallet.Amount += value;
        }

        public virtual void AddCost(Wallet wallet, decimal value, string description, Category category)
        {
            this.Guid = Guid.NewGuid();
            this.Value = value;
            this.Description = description;
            this.CreatedAt = DateTime.Now;
            this.TransactionType = TransactionType.Cost;

            this.Category = category;
            this.CategoryGuid = category.Guid;
            this.WalletGuid = wallet.Guid;
            this.Wallet = wallet;
            wallet.UpdatedAt = DateTime.Now;
            wallet.UpdatedAt = DateTime.Now;
            wallet.Amount -= value;
        }
    }
}
