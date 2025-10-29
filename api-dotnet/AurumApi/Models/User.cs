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

        public User(Guid guid)
        {
            this.Guid = guid;
        }

        public User(string name, string email, string phone, string pass = null)
        {
            Guid = Guid.NewGuid();
            this.Name = name;
            this.Email = email;
            this.PhoneNumber = phone;
            this.PasswordHash = pass;
        }

        public User(string phoneNumber)
        {
            this.PhoneNumber = phoneNumber;
        }

        public User(Guid guid, string name, string email, string phoneNumber)
        {
            Guid = guid;
            this.Name = name;
            this.Email = email;
            this.PhoneNumber = phoneNumber;
        }

        public void AddPass(Guid guid, string passwordHash)
        {
            this.Guid = guid;
            this.PasswordHash = passwordHash;
        }

        public void UpdateUser(string name, string email, string phone)
        {
            this.Name = name;
            this.Email = email;
            this.PhoneNumber = phone;
        }
    }
}
