using Aurum.Application.Interfaces.Auth;
using System.Security.Claims;

namespace Aurum.Api.Services.Auth
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?
            .User
            .Identity?
            .IsAuthenticated
        ?? false;

        public Guid UserId
        {
            get
            {
                var value =
                    _httpContextAccessor.HttpContext?
                        .User
                        .FindFirstValue(
                            ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(value, out var userId))
                {
                    throw new UnauthorizedAccessException(
                        "Usuário autenticado não identificado.");
                }

                return userId;
            }
        }
    }
}
