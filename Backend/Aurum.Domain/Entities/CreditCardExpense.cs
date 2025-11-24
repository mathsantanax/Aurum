using Aurum.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities
{
    public class CreditCardExpense
    {
        public Guid Id { get; private set; } // Identificador único da transação agendada
        public Guid WalletId { get; private set; }// Chave estrangeira para a carteira

        // Detalhes da transação agendada
        public Guid TransactiontGuid { get; private set; } // Chave estrangeira para a transação recorrente associada
        public Guid CreditCardGuid { get; private set; } // Chave estrangeira para o cartão de crédito associado
        public decimal Amount { get; private set; } // Valor da Parcela
        public int InstallmentNumber { get; private set; } // Número da parcela (se aplicável)
        public DateTime ScheduleExecutionDate { get; private set; } // Data programada para a execução da transação
        public Guid CategoryId { get; private set; }// Chave estrangeira para a categoria
        public ScheduledStatus Status { get; private set; } = ScheduledStatus.Pending;// Status da transação agendada (Pendente, Executada, Cancelada)
        public TransactionFlow Type { get; private set; } = TransactionFlow.Expense; // Tipo da transação (Despesa, Receita, Transferência)
        public PaymentMethod PaymentMethod { get; private set; } = PaymentMethod.CreditCard; // Método de pagamento (Cartão de Crédito)

        public CreditCardExpense() { } // Construtor para EF Core (protegido ou privado

        // Construtor público para criar uma nova transação agendada
        public CreditCardExpense(Guid walletId, Guid transactiontGuid, Guid creditCardGuid, decimal amount, int installmentNumber, DateTime scheduleExecutionDate, Guid categoryId)
        {
            this.Id = Guid.NewGuid();
            this.WalletId = walletId;
            this.TransactiontGuid = transactiontGuid;
            this.CreditCardGuid = creditCardGuid;
            this.Amount = amount;
            this.InstallmentNumber = installmentNumber;
            this.ScheduleExecutionDate = scheduleExecutionDate;
            this.CategoryId = categoryId;
            this.Type = TransactionFlow.Expense;
            this.Status = ScheduledStatus.Pending;
            this.PaymentMethod = PaymentMethod.CreditCard;
        }


        // Método para marcar a transação agendada como executada
        public void MarkAsExecuted()
        {
            if (this.Status != ScheduledStatus.Pending)
                throw new InvalidOperationException("A transação agendada já foi completada ou cancelada.");

            this.Status = ScheduledStatus.Completed;
        }

        // Método para cancelar a transação agendada
        public void MarkAsCancelled()
        {
            if (this.Status != ScheduledStatus.Pending)
                throw new InvalidOperationException("A transação agendada já foi executada.");

            this.Status = ScheduledStatus.Canceled;
        }
    }
}
