using Aurum.appli
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultDTO> LoginAsync(LoginDTO loginDto);
        Task<AuthResultDTO> RefreshTokensAsync(TokenRefreshDTO refreshDto);
    }
}
