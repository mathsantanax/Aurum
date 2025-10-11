using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Aurum_Domain.Entities
{
    public class User
    {
        [Key]
        public Guid Guid { get; private set; }
        [Required, MaxLength(100)]
        public string? FullName { get; private set; }
        [Required, MaxLength(200)]
        [EmailAddress]
        public string? Email { get; private set; }
        [MaxLength(20)]
        public string? Phone { get; private set; }
        [MaxLength(200)]
        public string? PasswordHash { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.Now;

        public virtual ICollection<PrivateWallet> PrivateWallets { get; set; } = new List<PrivateWallet>();
        public virtual ICollection<WalletMember> WalletMemberships { get; set; } = new List<WalletMember>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public User() { }

        public User(string fullName, string email, string phone)
        {
            Guid = Guid.NewGuid();
            FullName = fullName;
            Email = email;
            Phone = phone;  
        }

        public bool CanAccessWallet(Wallet wallet)
        {
            if (wallet is PrivateWallet pw)
                return pw.OwnerId == Id;

            if (wallet is SharedWallet sw)
                return sw.Members.Any(m => m.UserId == Id);

            return false;
        }
    }
}
