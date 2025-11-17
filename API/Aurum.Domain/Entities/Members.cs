using Aurum.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities
{
    public class Members 
    {
        public User? User { get; private set; }
        public Guid UserGuid { get; private set; }
        public Guid WalletGuid { get; private set; }
        public virtual SharedWallet SharedWallet { get; set; } = null!;

        public DateTime JoinedAt { get; private set; }
        public MemberRoles WalletRole { get; private set; } = MemberRoles.Member;
        public Members() { }

        public Members(Guid userGuid, Guid walletGuid, MemberRoles role = MemberRoles.Member)
        {
            this.UserGuid = userGuid;
            this.WalletGuid = walletGuid;
            this.WalletRole = role;
            this.JoinedAt = DateTime.UtcNow;
        }

        public void ChangeRole(MemberRoles newRole)
        {
            this.WalletRole = newRole;
        }
    }
}
