using Aurum_Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Entities
{
    public class Income
    {
        public Guid Id { get; private set; }
        public string? Description { get; private set; }
        public Money? Value { get; private set; }
        public DateTime Date { get; private set; }

        public Guid CategoryId { get; private set; }
        public Category? Category { get; private set; }

        public Guid WalletId { get; private set; }
        public Wallet? Wallet { get; private set; }

        private Income() { }

        public Income(string description, Money value, Category category)
        {
            Id = Guid.NewGuid();
            SetDescription(description);
            Value = value ?? throw new ArgumentException("Valor é obrigatório.");
            Category = category ?? throw new ArgumentException("Categoria é obrigatória.");
            CategoryId = category.Id;
            Date = DateTime.UtcNow;
        }

        private void SetDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Descrição é obrigatória.");
            Description = description;
        }

    }
}
