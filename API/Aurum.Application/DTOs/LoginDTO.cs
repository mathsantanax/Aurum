
using System.ComponentModel.DataAnnotations;

namespace Aurum.Application.DTOs
{
    // DTO para receber os dados de login
    public record class LoginDTO
    {
        [Required, StringLength(100)]
        public string Email { get; set; } = string.Empty!;
        [Required, StringLength(25)]
        public string Password { get; set; } = string.Empty!;
    }

    // DTO Para fazer cadastro de novos usuários
    public record class RegisterDTO
    {
        [Required, StringLength(150)]
        public string? Email { get; set; }
        [Required, StringLength(150)]
        public string? FullName { get; set; }
        [Required, StringLength(20)]
        public string? PhoneNumber { get; set; }
        [Required, StringLength(25)]
        public string? Password { get; set; }
        [Required, StringLength(25)]
        public string? ConfirmedPassword { get; set; }
    }

    // Novo DTO para o refresh de tokens
    public class TokenRefreshDTO
    {
        public required string Token { get; set; } // O JWT expirado (ou prestes a expirar)
        public required string RefreshToken { get; set; } // O Refresh Token salvo no BD
    }

    // DTO para retornar o resultado da autenticação, incluindo o token E o Refresh Token
    public class AuthResultDTO
    {
        public bool Success { get; set; }
        public string? Token { get; set; } // O novo JWT
        public string? RefreshToken { get; set; } // O novo Refresh Token
        public string? Message { get; set; }
        public Guid? UserId { get; set; } // Usando GUID
    }
}
