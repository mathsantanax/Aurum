using Aurum.Applications.DTOs;
using Aurum.Applications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aurum.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class WalletController(WalletService walletService) : ControllerBase
    {
        private readonly WalletService walletService = walletService;

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            // 1. Obtém o UserGuid do usuário logado (Claim Types e formatos podem variar)
            var userGuidClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userGuidClaim) || !Guid.TryParse(userGuidClaim, out var userGuid))
            {
                // Se o token de autenticação não fornecer um UserGuid válido
                return Unauthorized("Não foi possível identificar o usuário logado.");
            }

            // 2. Delega a lógica de negócio ao Serviço
            var wallets = await walletService.GetAllWalletsAsync(userGuid);

            // 3. Retorna o resultado (O Middleware trata quaisquer exceções lançadas pelo Serviço)
            return Ok(wallets);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] WalletDTO request)
        {
            // 1. Obtém o UserGuid do usuário logado (Claim Types e formatos podem variar)
            var userGuidClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userGuidClaim) || !Guid.TryParse(userGuidClaim, out var userGuid))
            {
                // Se o token de autenticação não fornecer um UserGuid válido
                return Unauthorized("Não foi possível identificar o usuário logado.");
            }
            // 2. Assegura que o UserGuid do request seja o do usuário logado
            // 3. Delega a lógica de negócio ao Serviço
            var createdWallet = await walletService.CreateWalletAsync(request, userGuid);
            // 4. Retorna o resultado (O Middleware trata quaisquer exceções lançadas pelo Serviço)
            return CreatedAtAction(nameof(Get), new { id = createdWallet.Id }, createdWallet);
        }

    }
}
