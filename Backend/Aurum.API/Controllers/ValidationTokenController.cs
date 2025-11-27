using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aurum.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ValidationTokenController : ControllerBase
    {
        [Authorize]
        [HttpGet("dados-protegidos")]
        public IActionResult GetDados()
        {
            // Extrai o ID do usuário (Guid) do token validado.
            // O .NET mapeia o 'sub' (Subject) para ClaimTypes.NameIdentifier
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdString, out Guid userId))
            {
                // Agora você pode usar esse userId para buscar dados SOMENTE desse usuário
                // ...
                return Ok(new { Message = $"Token validado com sucesso. ID do Usuário: {userId}" });
            }

            return Unauthorized();
        }
    }
}
