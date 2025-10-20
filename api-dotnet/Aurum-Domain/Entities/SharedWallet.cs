using Aurum_Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Entities
{
    public class SharedWallet : Wallet
    {
        [Required, MaxLength(255)]
        public string Description { get; set; } = string.Empty;
        public SharedWallet(string name, string description)
        {
            Name = name;
            Description = description;
            Type = WalletType.Public;
        }

        public bool CanAccess(User user, WalletRoles requiredRole = WalletRoles.Member)
        {
            var membership = Members.FirstOrDefault(m => m.UserGuid == user.Guid);
            if (membership == null) return false;
            return (WalletRoles)membership.Role >= requiredRole;
        }
    }
}
