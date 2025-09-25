using Aurum_Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Entities
{
    public abstract class Transaction
    {
        public Guid Id { get; private set; }
        public string? Description { get; private set; }
        public Money? Value { get; private set; }
        public DateTime Date { get; private set; }

        public Guid CategoryId { get; private set; }
        public Category? Category { get; private set; }

        public Guid? WalletId { get; private set; }
        public Wallet? Wallet { get; private set; }

        public Guid? SharedWalletId { get; private set; }
        public SharedWallet? SharedWallet { get; private set; }

        public abstract TransactionType Type { get; }

        protected Transaction() { }

        protected Transaction(string description, Money value, Category category, Wallet? wallet = null, SharedWallet? sharedWallet = null)
        {
            Id = Guid.NewGuid();
            SetDescription(description);
            Value = value ?? throw new ArgumentException("Valor é obrigatório.");
            Category = category ?? throw new ArgumentException("Categoria é obrigatória.");
            CategoryId = category.Id;
            Date = DateTime.UtcNow;

            if (wallet == null && sharedWallet == null)
                throw new InvalidOperationException("Transação deve estar vinculada a uma carteira.");

            Wallet = wallet;
            WalletId = wallet?.Id;
            SharedWallet = sharedWallet;
            SharedWalletId = sharedWallet?.Id;
        }

        private void SetDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Descrição é obrigatória.");
            Description = description;
        }
    }
}
