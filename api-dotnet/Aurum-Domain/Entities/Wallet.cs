using Aurum_Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Aurum_Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; private set; }
        public string? Name { get; private set; }
        public Money Balance { get; private set; }

        public Guid UserId { get; private set; }
        public User? User { get; private set; }

        private readonly List<Transaction> _transactions = new();
        public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

        private Wallet() { }

        public Wallet(string name, User user)
        {
            Id = Guid.NewGuid();
            SetName(name);
            Balance = Money.Zero();
            User = user ?? throw new ArgumentException("Usuário é obrigatório.");
            UserId = user.Id;
        }

        public void AddTransaction(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentException("Transação inválida.");

            if (transaction.Type == TransactionType.Income)
                Balance = Balance.Add(transaction.Value);
            else if (transaction.Type == TransactionType.Cost)
                Balance = Balance.Subtract(transaction.Value);

            _transactions.Add(transaction);
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome da carteira é obrigatório.");
            Name = name;
        }
    }
}
