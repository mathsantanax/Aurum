

namespace Aurum.Domain.Entities
{
    public class User
    {
        public Guid Guid { get; private set; }
        public string Name { get; private set; } = string.Empty!;
        public string Email { get; private set; } = string.Empty!;
        public string PhoneNumber { get; private set; } = string.Empty!;
        public string PasswordHash { get; private set; } = string.Empty!;

        public virtual ICollection<Wallet> Wallets { get; set; } = [];
        public User() { }

        public void AddUser(string name, string email, string phone)
        {
            Guid = Guid.NewGuid();
            this.Name = SetName(name);
            this.Email = email;
            this.PhoneNumber = phone;
            PasswordHash = string.Empty;
        }

        public void AddMember(Guid guid, string name)
        {
            this.Guid = guid;
            this.Name = SetName(name);
        }

        public void UpdateUser(Guid guid, string name, string email, string phone)
        {
            this.Guid = guid;
            this.Name= SetName(name);
            this.Email = email;
            this.PhoneNumber = phone;
        }

        public void GetUser(Guid? guid = null, string? phone = null)
        {
            if (guid == null && string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Você deve informar o GUID ou o número de telefone.");

            if (guid.HasValue)
                Guid = guid.Value;

            if (!string.IsNullOrWhiteSpace(phone))
                PhoneNumber = phone;
        }

        private string SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome é obrigatório.");
            return name;
        }
    }
}
