using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aurum.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TesteController : ControllerBase
    {
        /// Endpoint protegido por JWT. Só pode ser acessado com um token válido.
        [Authorize]
        [HttpGet("protected")]
        public IActionResult GetProtectedData()
        {

            // Tenta obter o ID do usuário (NameIdentifier) do token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(new
            {
                Message = "Este endpoint é protegido. Acesso concedido.",
                AuthenticatedUser = userEmail,
                UserId = userId,
                // O método User.Claims permite ver todas as informações carregadas do token
                ClaimsCount = User.Claims.Count(),
            });
        }
    }
}
