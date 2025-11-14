

using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Text.RegularExpressions;

namespace Aurum.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string fullName { get; private set; } = string.Empty!;
        public DateTime CreatedAt { get; private set; }
        public virtual ICollection<Wallet> Wallets { get; set; } = [];
        public User() { }

        public void RegisterUser(string name, string email, string phone, string pass, string confirmedPass)
        {
            this.Id = Guid.NewGuid();
            this.fullName = name.ToUpper();
            this.UserName = SetEmail(email);
            this.Email = SetEmail(email);
            this.PhoneNumber = SetPhoneNumber(phone);

            if (pass != confirmedPass)
                throw new InvalidOperationException("Senhas não são iguais.");

            this.PasswordHash = SetPassword(pass);
        }

        public void AddUser(string name, string email, string phone)
        {
            Id = Guid.NewGuid();
            this.CreatedAt = DateTime.UtcNow;
            this.UserName = SetName(name);
            this.Email = email;
            this.PhoneNumber = phone;
            PasswordHash = string.Empty;
        }

        public void AddMember(Guid guid, string name)
        {
            this.Id = guid;
            this.fullName = SetName(name);
        }

        public void UpdateUser(Guid guid, string name, string email, string phone)
        {
            this.Id = guid;
            this.fullName = SetName(name);
            this.Email = email;
            this.PhoneNumber = phone;
        }

        public void GetUser(Guid? guid = null, string? phone = null)
        {
            if (guid == null && string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Você deve informar o GUID ou o número de telefone.");

            if (guid.HasValue)
                Id = guid.Value;

            if (!string.IsNullOrWhiteSpace(phone))
                PhoneNumber = phone;
        }

        static string SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome é obrigatório.");
            return name;
        }

        static string SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("E-mail é obrigatório.");

            // Verificação de sintaxe básica
            string padrao = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email, padrao))
                throw new ArgumentException("E-mail inválido.");

            return email.ToLower();
        }

        static string SetPhoneNumber(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                throw new ArgumentException("Telefone é obrigatório.");

            // Remove caracteres não numéricos
            string numeroLimpo = Regex.Replace(numero, @"[^\d]", "");

            if (numeroLimpo.Length < 10 || numeroLimpo.Length > 11)
                throw new ArgumentException("Número de telefone inválido.");

            return numeroLimpo;
        }

        static string SetPassword(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
                throw new ArgumentException("Senha é obrigatória.");

            if (senha.Length < 8)
                throw new ArgumentException("Senha deve ter pelo menos 8 caracteres.");

            if (!Regex.IsMatch(senha, @"[A-Z]"))
                throw new ArgumentException("Senha deve conter pelo menos uma letra maiúscula.");

            if (!Regex.IsMatch(senha, @"[a-z]"))
                throw new ArgumentException("Senha deve conter pelo menos uma letra minúscula.");

            if (!Regex.IsMatch(senha, @"[0-9]"))
                throw new ArgumentException("Senha deve conter pelo menos um número.");

            if (!Regex.IsMatch(senha, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                throw new ArgumentException("Senha deve conter pelo menos um caractere especial.");

            return senha;
        }
    }
}
