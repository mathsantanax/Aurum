using Aurum.Application.DTOs;
using Aurum.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aurum.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PrivateWalletController : ControllerBase
    {
        private readonly PrivateWalletService privateWalletService;

        public PrivateWalletController(PrivateWalletService privateWalletService)
        {
            this.privateWalletService = privateWalletService;
        }
        /// Rota para criar uma carteira privada.
        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> CreatePrivateWallet([FromBody] WalletDto request)
        {
            if(string.IsNullOrEmpty(request.Name))
            {
                return BadRequest(new { message = "Nome da carteira é obrigatório." });
            }

            // Simulando a obtenção do GUID do usuário autenticado.
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sub))
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            var wallet = await privateWalletService.CreatePrivateWallet(request, Guid.Parse(sub));
            return Ok(wallet);
        }

        /// Rota para obter todas as carteiras privadas do usuário autenticado.
        [Authorize]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllPrivateWalletAsync()
        {
            // Simulando a obtenção do GUID do usuário autenticado.
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sub))
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }
            var wallets = await privateWalletService.GetAllPrivateWalletAsync(Guid.Parse(sub));
            return Ok(wallets);
        }

        /// Rota para obter uma carteira privada pelo ID.
        [Authorize]
        [HttpGet("GetById")]
        public async Task<IActionResult> GetPrivateWalletByIdAsync([FromBody] WalletDto wallet)
        {
            if (wallet.Id == Guid.Empty)
            {
                return BadRequest(new { message = "ID da carteira é obrigatório." });
            }

            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sub))
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }
            var foundWallet = await privateWalletService.GetPrivateWalletByIdAsync(wallet, Guid.Parse(sub));
            return Ok(foundWallet);
        }

        /// Rota para atualizar uma carteira privada.
        [Authorize]
        [HttpPut("Update")]
        public async Task<IActionResult> UpdatePrivateWalletAsync([FromBody] WalletDto wallet)
        {
            if (wallet.Id == Guid.Empty)
            {
                return BadRequest(new { message = "ID da carteira é obrigatório." });
            }
            if (string.IsNullOrEmpty(wallet.Name))
            {
                return BadRequest(new { message = "Nome da carteira é obrigatório." });
            }
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sub))
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }
            var updatedWallet = await privateWalletService.UpdatePrivateWalletAsync(wallet, Guid.Parse(sub));
            return Ok(updatedWallet);
        }

        /// Rota para deletar uma carteira privada.
        [Authorize]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeletePrivateWalletAsync([FromBody] WalletDto wallet)
        {
            if (wallet.Id == Guid.Empty)
            {
                return BadRequest(new { message = "ID da carteira é obrigatório." });
            }
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sub))
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }
            var result = await privateWalletService.DeletePrivateWalletAsync(wallet, Guid.Parse(sub));
            return Ok(new { success = result });
        }
    }
}
