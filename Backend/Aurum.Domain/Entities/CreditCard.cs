using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities
{
    public class CreditCard
    {
        public Guid Id { get; private set; } // Identificador único do cartão de crédito
        public Guid UserId { get; private set; } // Chave estrangeira para o usuário proprietário do cartão
        public Guid WalletId { get; private set; } // Chave estrangeira para a carteira associada
        public string? Name { get; private set; } // Nome do cartão de crédito
        public decimal CreditLimit { get; private set; } // Limite de crédito do cartão

        public int ClosingDay { get; private set; } // Dia de fechamento da fatura
        public int DueDay { get; private set; } // Dia de vencimento da fatura

        public decimal CurrentBillAmount { get; private set; } // Valor atual da fatura

        private readonly List<Transaction> transactions = [];
        public IReadOnlyCollection<Transaction> Transactions => transactions.AsReadOnly();

        public CreditCard() { } // Construtor para EF Core (protegido ou privado)

        // Construtor público para criar um novo cartão de crédito
        public CreditCard(Guid userId, Guid walletId, string? name, decimal creditLimit, int dueDay, int closingDay)
        {
            if (dueDay < 1 || dueDay > 31 || closingDay < 1 || closingDay > 31)
                throw new ArgumentException("O dia de vencimento e fechamento devem ser válidos (1-31).");

            this.Id = Guid.NewGuid();
            this.UserId = userId;
            this.WalletId = walletId;
            this.Name = name;
            this.CreditLimit = creditLimit;

            this.DueDay = dueDay;
            this.ClosingDay = closingDay;

            this.CurrentBillAmount = 0;
        }

        // Método para registrar uma despesa no cartão de crédito
        public void RegisterExpenseValue(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("O valor da despesa deve ser positivo.", nameof(amount));

            if (this.CurrentBillAmount + amount > CreditLimit)
                throw new InvalidOperationException("Limite de crédito excedido.");

            this.CurrentBillAmount += amount;
        }
    }
}
