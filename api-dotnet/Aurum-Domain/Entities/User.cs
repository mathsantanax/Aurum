using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Aurum_Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string? FullName { get; private set; }
        public string? Email { get; private set; }
        public string? Phone { get; private set; }

        private readonly List<Wallet> _wallets = new();
        public IReadOnlyCollection<Wallet> Wallets => _wallets.AsReadOnly();

        private User() { }

        public User(string name, string email, string phone)
        {
            Id = Guid.NewGuid();
            SetName(name);
            SetEmail(email);
            SetPhone(phone);
        }

        public void AddWallet(Wallet wallet)
        {
            _wallets.Add(wallet);
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
            Email = email;
        }

        private void SetPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Telefone é obrigatório.");
            Phone = phone;
        }
    }
}
