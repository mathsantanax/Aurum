using Aurum.Applications.DTOs;
using Aurum.Applications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aurum.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CreditCardController : Controller
    {
        private readonly CreditCardService _creditCardService;

        public CreditCardController(CreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }


        [HttpPost]
        [Authorize]
        [Route("{walletGuid:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] CreditCartDTO request,[FromRoute] Guid walletGuid)
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(user) || !Guid.TryParse(user, out var userGuid))
                // Se o token de autenticação não fornecer um UserGuid válido
                return Unauthorized("Não foi possível identificar o usuário logado.");

            if(walletGuid == Guid.Empty)
                return BadRequest($"Guid da carteira inválido. \n{walletGuid}");

            var creditCard =await _creditCardService.CreateCreditCard(userGuid, walletGuid, request);

            return Ok(creditCard);
        }
    }
}
