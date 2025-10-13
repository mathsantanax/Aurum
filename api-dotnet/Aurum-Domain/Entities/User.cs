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
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public User() { }

        public User(string fullName, string email, string phone)
        {
<<<<<<< HEAD
            Guid = Guid.NewGuid();
            SetName(fullName);
            SetEmail(email);
            SetPhone(phone);
        }

        public User(Guid id, string? fullName, string? email, string? phone)
        {
            SetName(fullName);
            SetEmail(email);
            SetPhone(phone);
        } 

        public void setGuid(Guid guid)
        {
            if (guid == Guid.Empty)
                throw new ArgumentNullException(nameof(guid), "Guid não pode ser nulo");
            Id = guid;
        }

        public void AddWallet(Wallet wallet)
        {
            _wallets.Add(wallet);
        }

        public void AddSahredWallet(SharedWallet shared)
        {
            _sharedWallets.Add(shared);
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome é obrigatório.");
            FullName = name;
        }

        private void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("E-mail inválido.");
=======
            Guid = Guid.NewGuid();
            FullName = fullName;
>>>>>>> 00b5558a5b7f090bb0b2570ca70ae5de3b114aa8
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
