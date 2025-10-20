using System.ComponentModel.DataAnnotations;

namespace AurumApi.Models
{
    public class User
    {
        [Key]
        public Guid Guid { get; set; }
        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty!;
        [Required, StringLength(100)]
        public string Email { get; private set; } = string.Empty!;
        [Required, StringLength(20)]
        public string PhoneNumber { get; private set; } = string.Empty!;
        public string PasswordHash { get; private set; }
        public virtual ICollection<Wallet> Wallets { get; set; } = [];

        public User() { }

        public User(string name, string email, string phone, string passwordHash)
        {
            Guid = Guid.NewGuid();
            this.Name = name;
            this.Email = email;
            this.PhoneNumber = phone;
            PasswordHash = passwordHash;
        }

        public User(Guid guid, string name, string email, string phoneNumber, string passwordHash)
        {
            Guid = guid;
            this.Name = name;
            this.Email = email;
            this.PhoneNumber = phoneNumber;
            PasswordHash = passwordHash;
        }
    }
}
