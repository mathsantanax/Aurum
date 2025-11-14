using Aurum.Application.DTOs;

namespace Aurum.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string email, string password);
        Task<LoginResponse> RefreshTokenAsync(string refreshToken);
        Task LoginOutAsync(string refreshToken);
        Task<LoginResponse> ExternalLoginAsync(string provider, string idToken);
    }
}
