using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.ValueObjects
{
    public sealed class Money
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        private Money() { } // EF Core

        public Money(decimal amount, string currency = "BRL")
        {
            if (amount < 0)
                throw new ArgumentException("O valor não pode ser negativo.");

            Amount = decimal.Round(amount, 2);
            Currency = currency.ToUpper();
        }

        public static Money Zero(string currency = "BRL") => new Money(0, currency);

        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            if (Amount - other.Amount < 0)
                throw new InvalidOperationException("Saldo insuficiente.");
            return new Money(Amount - other.Amount, Currency);
        }

        private void EnsureSameCurrency(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Moedas diferentes não podem ser somadas/subtraídas.");
        }

        public override string ToString() => $"{Currency} {Amount:N2}";

    }
}
