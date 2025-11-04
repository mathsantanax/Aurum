using Aurum.Application.DTOs;
using Aurum.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Aurum.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // GET /api/v1/users/me  -> retorna usuário autenticado com wallets
        [HttpGet("me")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var idClaim = User.FindFirstValue(JwtRegisteredClaimNames.Jti);

                if (string.IsNullOrEmpty(idClaim))
                    return Unauthorized("Token inválido: jti ausente.");

                if (!Guid.TryParse(idClaim, out var userGuid))
                    return BadRequest("jti não contém um Guid válido.");

                var user = await _userService.GetUserAsync(
                            new UserDTO
                            {
                                Guid = userGuid,
                                Email = User.FindFirstValue(ClaimTypes.Email)!

                            });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserDTO request)
        {
            await _userService.AddUserAsync(request);
            return Ok("Cadastrado Com Sucesso!");
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UserDTO request)
        {
            await _userService.UpdateUserAsync(request);
            return Ok("Usuário Atualizado com Sucesso!");
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] UserDTO request)
        {
            await _userService.DeleteUserAsync(request);
            return Ok("Usuário Deletado com sucesso!");
        }
    }
}
