using Aurum.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Aurum.Api.Configuration;

namespace Aurum.Api.Service
{
    public sealed class AuthService
    {
        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Globals.JWT_TOKEN.Trim())!);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
                        {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.PhoneNumber, user.PhoneNumber!),
            };

            var token = new JwtSecurityToken(
                issuer: Globals.JWT_ISSUER,
                audience: Globals.JWT_AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Globals.EXPIRE_IN_MINUTES),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
