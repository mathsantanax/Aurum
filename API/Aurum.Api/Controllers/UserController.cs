using Aurum.Application.DTOs;
using Aurum.Application.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Aurum.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromBody] UserDTO request)
        {
            try
            {
                return Ok(await _userService.GetUserAsync(request));
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
