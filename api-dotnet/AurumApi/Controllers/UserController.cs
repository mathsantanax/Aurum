using AurumApi.Dtos;
using AurumApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace AurumApi.Controllers
{
    [ApiController]
    [Route("api/")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserDTO request)
        {
            await _userService.AddUser(request);
            return Ok("Usuário Cadastrado com sucesso");
        }


        [HttpGet("id")]
        public async Task<IActionResult> GetUser([FromBody] Guid id)
        {
            var returnUser = await _userService.GetUser(id);
            return Ok(returnUser);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UserDTO request)
        {
            await _userService.UpdateUser(request);
            return Ok("Usuário Atualizado com Sucesso!");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] UserDTO request)
        {
            await _userService.DeleteUser(request);
            return NotFound();
        }


    }
}
