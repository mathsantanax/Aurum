using System.ComponentModel.DataAnnotations.Schema;
using AurumApi.Models.Enums;

namespace AurumApi.Models
{
    public class PrivateWallet : Wallet
    {
        [ForeignKey("OwnerGuid")]
        public Guid OwnerGuid { get; set; }
        public virtual User User { get; set; } = null!;

        public PrivateWallet() { }

        public PrivateWallet(string name, User ownerUser)
        {
            this.Guid = Guid.NewGuid();
            this.OwnerGuid = ownerUser.Guid;
            this.Name = name;
            this.WalletType = WalletType.Private;
            this.CreatedAt = DateTime.UtcNow;
        }
    }
}
