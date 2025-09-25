using Aurum_Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Entities
{
    public class SharedWallet
    {
        public Guid Id { get; private set; }
        public string? Name { get; private set; }
        public Money? Balance { get; private set; }
        public Guid OwnerId { get; set; }
        public User? Owner { get; set; }

        private readonly List<User> _members = new();
        public IReadOnlyCollection<User> Members => _members;

        private readonly List<Transaction> _transactions = new();
        public IReadOnlyCollection<Transaction> Transactions => _transactions;

        private SharedWallet() { }

        public SharedWallet(string name, IEnumerable<User> members)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentException("Nome é obrigatório.");
            Balance = Money.Zero();

            if (members == null || !members.Any())
                throw new ArgumentException("Uma carteira compartilhada precisa ter pelo menos um membro.");

            _members.AddRange(members);
        }

        public void AddMember(User user)
        {
            if (_members.Any(x => x.Id == user.Id))
                throw new InvalidOperationException("Usuário já é membro da carteira compartilhada.");
            _members.Add(user);
        }

        public void AddTransaction(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentException("Transação inválida.");

            if (transaction.Type == TransactionType.Income)
                Balance = Balance!.Add(transaction.Value);
            else if (transaction.Type == TransactionType.Cost)
                Balance = Balance!.Subtract(transaction.Value);

            _transactions.Add(transaction);
        }
    }

}
