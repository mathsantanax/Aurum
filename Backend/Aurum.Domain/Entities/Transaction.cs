using Aurum.Domain.Entities.Enums;

namespace Aurum.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; private set; } // Identificador único da transação
        public Guid UserGuid { get; private set; } // Chave estrangeira para o usuário que realizou a transação
        public Guid WalletId { get; private set; } // Chave estrangeira para a carteira

        // Detalhes da transação
        public decimal Amount { get; private set; } // Valor da transação
        public DateTime CreatedDate { get; private set; } // Data da transação
        public string? Description { get; private set; } // Descrição da transação
        public TransactionFlow Type { get; private set; } // Tipo da transação (Despesa, Receita, Transferência)
        public Guid CategoryId { get; private set; } // Chave estrangeira para a categoria
        public int? InstallmentNumber { get; private set; } // Número da parcela (se aplicável)
        public Guid? CreditCardGuid { get; private set; } // Chave estrangeira para o cartão de crédito (se aplicável)
        public PaymentMethod PaymentMethod { get; private set; } // Método de pagamento

        private readonly List<CreditCardExpense> _creditCardExpenses = [];
        public IReadOnlyCollection<CreditCardExpense> CreditCardExpenses => _creditCardExpenses.AsReadOnly();


        // Construtor para EF Core (protegido ou privado)
        public Transaction() { } 

        // Construtor público para criar uma nova transação
        public Transaction(Guid walletId, Guid categoryId, decimal amount, string? description, TransactionFlow type, Guid userGuid)
        {
            if (amount <= 0)
                throw new ArgumentException("O valor da transação deve ser maior que zero.", nameof(amount));

            this.Id = Guid.NewGuid();
            this.WalletId = walletId;
            this.CategoryId = categoryId;
            this.Amount = amount;
            this.Description = description;
            this.Type = type;
            this.CreatedDate = DateTime.UtcNow;
            this.UserGuid = userGuid;

            this.InstallmentNumber = null;
            this.CreditCardGuid = null;
        }

        public void AddCreditExpenses(CreditCardExpense creditCardExpenses)
        {
            ArgumentNullException.ThrowIfNull(creditCardExpenses); // Verifica se o crédito é nulo
            _creditCardExpenses.Add(creditCardExpenses);
        }

        public void RegisterCreditCardTransaction(Guid creditCardGuid, int installmentNumber)
        {
            this.CreditCardGuid = creditCardGuid;
            if (installmentNumber <= 1)
                throw new ArgumentException("O número da parcela deve ser maior que zero.", nameof(installmentNumber));
            this.InstallmentNumber = installmentNumber;
        }
    }
}
