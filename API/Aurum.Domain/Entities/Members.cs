using Aurum.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities
{
    public class Members : User
    {
        [ForeignKey("Wallet")]
        public Guid WalletGuid { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public MemberRoles WalletRole { get; private set; } = MemberRoles.Member;
        public virtual SharedWallet SharedWallet { get; set; } = null!;

        public Members() { }

        public Members(User user, SharedWallet wallet, MemberRoles roles = MemberRoles.Member)
        {
            user.AddMember(user.Id, user.fullName);

            this.WalletGuid = wallet.Guid;
            this.WalletRole = roles;
            this.JoinedAt = DateTime.UtcNow;

        }
    }
}
