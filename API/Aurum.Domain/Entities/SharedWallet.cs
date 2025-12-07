using Aurum.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities
{
    public class SharedWallet : Wallet
    {
        public Guid OwnerGuide { get; private set; }
        public User OwnerUser { get; private set; } = null!;

        public virtual ICollection<Members> Members { get; set; } = new List<Members>();

        public SharedWallet() { }


        // Construtor para criação de nova carteira
        public SharedWallet(string name, User owner)
        {
            this.Guid = Guid.NewGuid();
            this.Name = name;
            this.WalletType = WalletType.Public;
            this.CreatedAt = DateTime.UtcNow;
            this.Amount = 0; 

            this.OwnerGuide = owner.Id;
            this.OwnerUser = owner;

            // REGRA DE NEGÓCIO: Adiciona o criador como o primeiro membro com ROLE DE ADMIN.
            // O construtor de Members agora aceita GUIDs.
            var adminMember = new Members(owner.Id, this.Guid, MemberRoles.Admin);
            this.Members.Add(adminMember);
        }

        public void SetNullOwner()
        {
            this.OwnerUser = null!;
        }

        public void ChangeNameWallet(string name)
        {
            this.Name = name;
        }

        public string AddMember(User user, MemberRoles role = MemberRoles.Member)
        {
            // Validação de Duplicidade: Verifica se já existe um registro Members para este UserGuid
            if (Members.Any(m => m.UserGuid == user.Id))
                return $"Usuário {user.fullName} já é membro da carteira.";

            // Cria e adiciona a nova entidade de junção (Members)
            var newMember = new Members(user.Id, this.Guid, role);

            Members.Add(newMember);

            return $"Usuário {user.fullName} adicionado como {role}.";
        }

        // Você pode adicionar métodos aqui para gerenciar ou verificar permissões:
        public bool CanUserModify(Guid userId)
        {
            var member = Members.FirstOrDefault(m => m.UserGuid == userId);
            return member != null && member.WalletRole == MemberRoles.Admin;
        }
        
    }
}

