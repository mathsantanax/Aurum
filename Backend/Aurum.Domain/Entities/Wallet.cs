using Aurum.Domain.Entities.Enums;

namespace Aurum.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; private set; } // Identificador único da carteira
        public string? Name { get; private set; } // Nome da carteira

        private decimal _balance; // Saldo da carteira
        public decimal Balance => _balance; // Saldo da carteira
        public DateTime CreatedAt { get; private set; } // Data de criação
        public DateTime UpdatedAt { get; private set; } // Data da última atualização

        private readonly List<Transaction> _transactions = []; // Lista de transações associadas à carteira
        public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly(); // Exposição somente leitura das transações

        private readonly List<SharedWalletMembership> _sharedWalletMemberships = []; // Lista de membros compartilhados
        public IReadOnlyCollection<SharedWalletMembership> SharedWalletMemberships => _sharedWalletMemberships.AsReadOnly(); // Exposição somente leitura dos membros compartilhados

        public Guid OwnerGuid { get; private set; } // Chave estrangeira para o proprietário da carteira

        // Construtor para EF Core (protegido ou privado)
        public Wallet() { }

        // Construtor público para criar uma nova carteira
        public Wallet(Guid OwnerUserGuid, string? name)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this._balance = 0;
            this.OwnerGuid = OwnerUserGuid;
            this.CreatedAt = DateTime.UtcNow;
            this.UpdatedAt = DateTime.UtcNow;

            // Adiciona o proprietário como o primeiro membro com papel de dono
            AddMembership(OwnerUserGuid, MemberRole.Owner,new ContributionRule(ContributionType.None, 0));
        }

        public void AddTransaction(Transaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            // Verifica se o usuário que está adicionando a transação é um membro da carteira
            var member = SharedWalletMemberships.FirstOrDefault(m => m.UserGuid == transaction.UserGuid) ?? throw new InvalidOperationException("Usuário não é membro desta carteira.");

            // Verifica se o membro tem permissão para adicionar transações
            if (member.Role == MemberRole.Viewer)
                throw new InvalidOperationException("Usuário com permissão de visualização não pode adicionar transações.");
            
            // Verifica se a transação pertence a esta carteira
            if (transaction.WalletId != Id)
                throw new InvalidOperationException("A transação não pertence a esta carteira.");

            // Verifica se o valor da transação é válido
            if (transaction.Amount <= 0)
                throw new InvalidOperationException($"O valor da transação deve ser maior que zero. {transaction.Amount}");
            

            _transactions.Add(transaction);
            if(!transaction.InstallmentNumber.HasValue && transaction.InstallmentNumber !> 1)
                UpdateBalance(transaction.Type, transaction.Amount);
        }

        // Método de Domínio para adicionar o primeiro membro (o dono)
        private void AddMembership(Guid userId, MemberRole role, ContributionRule rule)
        {
            if (SharedWalletMemberships.Any(m => m.UserGuid == userId)) 
            {
                throw new InvalidOperationException("Usuário já é membro desta carteira.");
            }

            var membership = new SharedWalletMembership(userId, Id, role, rule);
            _sharedWalletMemberships.Add(membership);
        }

        public void ChangeRoleMember(Guid userId, MemberRole newRole, Guid ChangeUserGuid)
        {
            var membership = _sharedWalletMemberships.FirstOrDefault(m => m.UserGuid == userId) ?? throw new InvalidOperationException("Usuário não é membro desta carteira.");
            var changingMember = _sharedWalletMemberships.FirstOrDefault(m => m.UserGuid == ChangeUserGuid) ?? throw new InvalidOperationException("Usuário que está alterando o papel não é membro desta carteira.");

            // Verifica se o membro que está tentando alterar o papel tem permissão
            if (membership.Role == MemberRole.Owner && membership.Role == MemberRole.Admin)
                throw new InvalidOperationException("Permissão negada. Apenas Proprietários ou Administradores podem alterar papéis.");

            // Verifica se o membro que está sendo alterado é o proprietário
            if (changingMember.Id.Equals(ChangeUserGuid))
                throw new InvalidOperationException("Não é possível alterar o proprietário da carteira!");

            // Verifica se o novo papel é o de proprietário
            if (newRole == MemberRole.Owner)
                throw new InvalidOperationException("Não é possível alterar o usuário para proprietário da carteira!");

            if(membership == changingMember)
                throw new InvalidOperationException("Usuário não pode alterar seu próprio papel.");

            changingMember.ChangeRole(newRole);
            this.UpdatedAt = DateTime.UtcNow;
        }

        // Método de Domínio para adicionar membros aceitos
        public void AddAcceptMember(Guid userId, MemberRole role, ContributionRule rule)
        {
            if (SharedWalletMemberships.Any(m => m.UserGuid == userId))
                throw new InvalidOperationException("Usuário já é membro desta carteira.");
            

            var membership = new SharedWalletMembership(userId, Id, role, rule);
            _sharedWalletMemberships.Add(membership);
        }

        // Método para atualizar o saldo da carteira
        private void UpdateBalance(TransactionFlow type, decimal amount)
        {
            if (type == TransactionFlow.Expense)
                this._balance -= amount;
            if (type == TransactionFlow.Income)
                this._balance += amount;
            else if(type == TransactionFlow.Transfer)
                this._balance -= amount;

            this.UpdatedAt = DateTime.UtcNow;
        }

    }
}
