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
        public Guid WalletId { get; private set; } // Chave estrangeira para a carteira associada
        public string? Name { get; private set; } // Nome do cartão de crédito
        public decimal CreditLimit { get; private set; } // Limite de crédito do cartão
        public DateTime ClosingDay { get; private set; } // Dia de fechamento da fatura
        public DateTime DueDay { get; private set; } // Dia de vencimento da fatura
        
        public decimal CurrentBillAmount { get; private set; } // Valor atual da fatura

        private readonly List<Transaction> _expenses = []; // Lista de despesas associadas ao cartão de crédito
        public IReadOnlyCollection<Transaction> Expenses => _expenses.AsReadOnly(); // Exposição somente leitura das despesas

        public CreditCard() { } // Construtor para EF Core (protegido ou privado)

        // Construtor público para criar um novo cartão de crédito
        public CreditCard(Guid walletId, string? name, decimal creditLimit, DateTime closingDay, DateTime dueDay)
        {
            this.Id = Guid.NewGuid();
            this.WalletId = walletId;
            this.Name = name;
            this.CreditLimit = creditLimit;
            this.ClosingDay = closingDay;
            this.DueDay = dueDay;
            this.CurrentBillAmount = 0;
        }

        public void RegisterExpense(Transaction transaction)
        {
            // Verifica se o valor da despesa é válido
            if (transaction.Amount <= 0)
                throw new ArgumentException("O valor da despesa deve ser maior que zero.", nameof(transaction.Amount));

            // Verifica se o valor da despesa excede o limite de crédito
            if (this.CurrentBillAmount + transaction.Amount > CreditLimit)
                throw new InvalidOperationException("Limite de crédito excedido.");

            this.CurrentBillAmount += transaction.Amount;
        }
    }
}
