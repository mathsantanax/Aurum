using AurumApi.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AurumApi.Models
{
    public class Members : User
    {
        [ForeignKey("Wallet")]
        public Guid WalletGuid { get; set; }
        public DateTime JoinedAt { get; set; }

        public MemberRoles WalletRoles { get; set; } = MemberRoles.Member;

        public virtual SharedWallet SharedWallet { get; set; } = null!;

        public Members() { }

        public Members(User user, SharedWallet wallet, MemberRoles roles = MemberRoles.Member)
        {
            this.Guid = user.Guid;
            this.Name = user.Name;

            this.WalletGuid = wallet.Guid;
            this.WalletRoles = roles;
            this.JoinedAt = DateTime.UtcNow;

        }
    }
}
