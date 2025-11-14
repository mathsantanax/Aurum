using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Application.DTOs
{
    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string Token, DateTime ExpiresAt, string RefreshToken);
    public record RefreshRequest(string RefreshToken);
    public record ExternalLoginRequest(string Provider, string IdToken);
}
