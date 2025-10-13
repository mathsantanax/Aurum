using Aurum_Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Entities
{
    public class PrivateWallet : Wallet
    {
        [ForeignKey("Owner")]
        public Guid OwnerGuid { get; set; }
        public virtual User Owner { get; set; } = null!;

        public PrivateWallet(string name, User owner)
        {
            Name = name;
            Owner = owner;
            OwnerGuid = owner.Guid;
            Type = WalletType.Private;
        }

        public bool CanAccess(User user) => user.Guid == OwnerGuid;
    }
}
