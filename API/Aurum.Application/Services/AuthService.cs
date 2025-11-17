using Aurum.Application.DTOs;
using Aurum.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Application.Services
{
    public class AuthService
    {
        private readonly UserManager<User> _userManager;

        public AuthService(UserManager<User> userManager)
        {
            this._userManager = userManager;
        }

        public async Task<User> RegisterUser(RegisterDTO request)
        {
            var user = new User();
            user.RegisterUser(request.FullName, request.Email, request.PhoneNumber, request.Password, request.ConfirmedPassword);
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new ApplicationException($"Erro ao registrar usuário: {errors}");
            }
            return user;
        }

        public async Task<string> AuthenticateAndGenerateToken(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ApplicationException("Email não pode ser nullo.");

            var ValidatedUser = await _userManager.FindByEmailAsync(email);
            if (ValidatedUser == null)
                throw new ApplicationException("Usuário não encontrado!");

            var isValidPassword = await _userManager.CheckPasswordAsync(ValidatedUser, password);
            if (isValidPassword)
            {
                var refreshToken = GenerateRefreshToken();

                ValidatedUser.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

                await _userManager.UpdateAsync(ValidatedUser);

                var jwtToken = GenerateJwtToken(ValidatedUser);

                return jwtToken;
            }

            throw new ApplicationException("Email ou Senha incorretos!");
        }

        public async Task<(string? newJwt, string? newRefreshToken)> RefreshTokens(string expiredJwt, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(expiredJwt);
            if (principal == null)
                throw new ApplicationException("Token inválido.");
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new ApplicationException("Token inválido.");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new ApplicationException("Refresh token inválido ou expirado.");
            var newJwtToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
            return (newJwtToken, newRefreshToken);
        }

        // Extrai as Claims de um token expirado
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                // ... (as mesmas configurações do BuilderExtension)
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // ESSENCIAL: Ignorar a expiração do token
                ValidateIssuerSigningKey = true,
                ValidIssuer = Environment.GetEnvironmentVariable("ISSUER"),
                ValidAudience = Environment.GetEnvironmentVariable("AUDIENCE"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("KEY")!))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            // Validação extra de segurança
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Token de acesso inválido ou algoritmo incorreto.");
            }
            return principal;
        }


        // Gera um Refresh Token seguro
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private string GenerateJwtToken(User user)
        {
            // 1. Obter a chave e configurações das variáveis de ambiente
            var jwtSecret = Environment.GetEnvironmentVariable("KEY")!;
            var issuer = Environment.GetEnvironmentVariable("ISSUER");
            var audience = Environment.GetEnvironmentVariable("AUDIENCE");
            var expireInMinutes = Convert.ToInt32(Environment.GetEnvironmentVariable("EXPIREINMINUTES"));

            // 2. Definir as Claims (informações sobre o usuário no token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),                  
                new Claim(ClaimTypes.Name, user.fullName),                
                // new Claim(ClaimTypes.Role, "Admin"), // Adicione roles se aplicável
            };

            // 3. Criar a chave de segurança
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Configurar o Token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.Now.AddDays(expireInMinutes), // Token expira em 7 dias
                SigningCredentials = credentials
            };

            // 5. Gerar e Serializar
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
