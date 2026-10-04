
using Aurum.Domain.Enums;

namespace Aurum.Domain.Entities.Workspace
{
    public class Walletspace : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        private readonly List<WalletspaceMember> _members = [];
        public IReadOnlyCollection<WalletspaceMember> Members => _members;

        private Walletspace()
        {
        }

        public Walletspace(string name, Guid createdBy)
        {
            if (createdBy == Guid.Empty)
                throw new ArgumentException(
                    "O usuário proprietário é obrigatório.",
                    nameof(createdBy));

            SetName(name);
            SetCreatedInfo(createdBy);

            var owner = new WalletspaceMember(
                Id,
                createdBy,
                WalletspaceRole.Owner);

            _members.Add(owner);
        }


        public void Rename(string newName, Guid updatedBy)
        {
            SetName(newName);
            MarkAsUpdated(updatedBy);
        }

        public void AddMember(WalletspaceMember member)
        {
            ArgumentNullException.ThrowIfNull(member);
            if (member.WalletspaceId != Id)
                throw new ArgumentException("O membro pertence a outro Walletspace.", nameof(member));
            if (_members.Any(existing => existing.UserId == member.UserId))
                throw new InvalidOperationException("O usuário já é membro do Walletspace.");
            if (member.Role == WalletspaceRole.Owner)
                throw new InvalidOperationException("A propriedade só pode ser transferida para um membro existente.");

            _members.Add(member);
        }

        public void ChangeMemberRole(Guid memberUserId, WalletspaceRole role, Guid updatedBy)
        {
            if (!Enum.IsDefined(role) || role == WalletspaceRole.Owner)
                throw new ArgumentOutOfRangeException(nameof(role));

            var member = _members.SingleOrDefault(item => item.UserId == memberUserId)
                ?? throw new ArgumentException("O usuário não é membro do Walletspace.", nameof(memberUserId));
            if (member.Role == WalletspaceRole.Owner)
                throw new InvalidOperationException("A propriedade só pode ser alterada por transferência.");

            member.ChangeRole(role, updatedBy);
            MarkAsUpdated(updatedBy);
        }

        public void TransferOwnership(Guid newOwnerId, Guid updatedBy)
        {
            if (newOwnerId == Guid.Empty)
                throw new ArgumentException("O novo proprietário é obrigatório.", nameof(newOwnerId));
            if (updatedBy == Guid.Empty)
                throw new ArgumentException("O responsável pela transferência é obrigatório.", nameof(updatedBy));

            var currentOwner = _members.SingleOrDefault(member => member.Role == WalletspaceRole.Owner)
                ?? throw new InvalidOperationException("O Walletspace não possui um proprietário.");
            if (currentOwner.UserId == newOwnerId)
                throw new ArgumentException("O usuário já é proprietário.", nameof(newOwnerId));
            var newOwner = _members.SingleOrDefault(member => member.UserId == newOwnerId)
                ?? throw new ArgumentException("O novo proprietário deve ser membro do Walletspace.", nameof(newOwnerId));

            newOwner.ChangeRole(WalletspaceRole.Owner, updatedBy);
            currentOwner.ChangeRole(WalletspaceRole.Admin, updatedBy);
            MarkAsUpdated(updatedBy);
        }

        public void SetName(string newName)
        {
            // 1. Nulo ou Espaço em Branco
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("O nome é obrigatório e não pode conter apenas espaços.");

            // 2. Sanitização interna do domínio
            newName = newName.Trim();

            // 3. Limites de tamanho
            if (newName.Length < 3 || newName.Length > 100)
                throw new ArgumentException("O nome deve ter entre 3 e 100 caracteres.");

            Name = newName;
        }
    }
}
