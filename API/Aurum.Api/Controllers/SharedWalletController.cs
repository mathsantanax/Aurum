using Aurum.Application.DTOs;
using Aurum.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Aurum.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SharedWalletController : ControllerBase
    {
        private readonly SharedWalletService _sharedWalletService;

        public SharedWalletController(SharedWalletService sharedWalletService)
        {
            _sharedWalletService = sharedWalletService;
        }

        [Authorize]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetSharedWallets()
        {
            var userGuid = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if(userGuid == Guid.Empty)
                return Unauthorized(new { message = "Usuário não autenticado." });

            var wallets = await _sharedWalletService.GetAllSharedWallets(userGuid);

            return Ok(wallets);
        }

        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateSharedWallet([FromBody] WalletDto request)
        {
            var userGuid = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (userGuid == Guid.Empty)
                return Unauthorized(new { message = "Usuário não autenticado." });
            var wallet = await _sharedWalletService.CreateSharedWallet(request, userGuid);
            return Ok(wallet);
        }

        [Authorize]
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateSharedWallet([FromBody] WalletDto request)
        {
            var userGuid = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (userGuid == Guid.Empty)
                return Unauthorized(new { message = "Usuário não autenticado." });
            var wallet = await _sharedWalletService.UpdateSharedWallet(request, userGuid);
            return Ok(wallet);
        }

        [Authorize]
        [HttpGet("GetById/{guid}")]
        public async Task<IActionResult> GetSharedWalletById([FromRoute] Guid guid)
        {
            var userGuid = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (userGuid == Guid.Empty)
                return Unauthorized(new { message = "Usuário não autenticado." });
            var wallet = await _sharedWalletService.GetSharedWalletById(guid, userGuid);
            if (wallet == null)
                return NotFound(new { message = "Carteira compartilhada não encontrada." });
            return Ok(wallet);
        }
    }
}
