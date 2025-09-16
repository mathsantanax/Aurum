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
        public string Name { get; private set; }
        public Money Balance { get; private set; }

        private readonly List<Income> _incomes = new();
        private readonly List<Cost> _costs = new();

        public IReadOnlyCollection<Income> Incomes => _incomes.AsReadOnly();
        public IReadOnlyCollection<Cost> Costs => _costs.AsReadOnly();

        private Wallet() { }

        public Wallet(string name)
        {
            Id = Guid.NewGuid();
            SetName(name);
            Balance = Money.Zero();
        }

        public void AddIncome(Income income)
        {
            _incomes.Add(income);
            Balance = Balance.Add(income.Value);
        }

        public void AddCost(Cost cost)
        {
            _costs.Add(cost);
            Balance = Balance.Subtract(cost.Value);
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome da carteira é obrigatório.");
            Name = name;
        }

    }
}
