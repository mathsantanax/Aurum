using Aurum.Application.DTOs;
using Aurum.Domain.Entities;
using System.Security.Claims;

namespace Aurum.Application.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
