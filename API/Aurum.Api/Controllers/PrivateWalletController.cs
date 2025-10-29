using Aurum.Application.DTOs;
using Aurum.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrivateWalletController : ControllerBase
    {
        private readonly PrivateWalletService privateWalletService;

        public PrivateWalletController(PrivateWalletService privateWalletService)
        {
            this.privateWalletService = privateWalletService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PrivateWalletDTO request)
        {

            try
            {
                await privateWalletService.AddWallet(request);
                return Ok("Carteira Criada");
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
    }
}
