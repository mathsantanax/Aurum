using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Entities
{
    public class WalletMember
    {
        [ForeignKey("Wallet")]
        public Guid WalletGuid { get; set; }

        [ForeignKey("User")]
        public Guid UserGuid { get; set; }

        public int Role { get; set; }  // 1=Owner, 2=Member

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public virtual SharedWallet Wallet { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
