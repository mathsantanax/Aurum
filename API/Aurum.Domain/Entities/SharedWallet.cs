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
        public Guid OwnerGuide { get; private set; } = Guid.Empty!;
        public User OwnerUser { get; private set; } = null!;

        public virtual ICollection<Members> Members { get; set; } = [];

        public SharedWallet() { }



        public SharedWallet(string name, User owner)
        {
            this.Guid = Guid.NewGuid();
            this.Name = name;
            this.WalletType = WalletType.Public;
            this.CreatedAt = DateTime.Now;

            this.OwnerGuide = owner.Id;
            this.OwnerUser = owner;

            this.Members.Add(new Members(owner, this, MemberRoles.Admin));
        }

        public string AddMember(User user, MemberRoles role = MemberRoles.Member)
        {
            if (Members.Any(m => m.Id == user.Id))
                return $"Usuário {user.fullName} já é membro da carteira.";

            Members.Add(new Members(user, this, role));
            return $"Usuário {user.fullName} adicionado como {role}.";
        }
    }
}
