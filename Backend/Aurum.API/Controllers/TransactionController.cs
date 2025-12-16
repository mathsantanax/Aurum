using Aurum.Applications.DTOs;
using Aurum.Applications.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aurum.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;
        public TransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] TransactionDTO request)
        {
            try
            {
                var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(user) || !Guid.TryParse(user, out var userGuid))
                    // Se o token de autenticação não fornecer um UserGuid válido
                    return Unauthorized("Não foi possível identificar o usuário logado.");

                await _transactionService.AddTransactionAsync(request, userGuid);

                return Ok("Transação adicionada com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
