
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
