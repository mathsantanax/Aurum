using Aurum_Application.DTOs;
using Aurum_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AurumApi.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        #region GetUser
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var user = await userService.GetUserAsync(id);
            return Ok(user);
        }

        #endregion

        #region AddUser
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] UserDto user)
        {
            await userService.AddUserAsync(user);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        #endregion

        #region PutUser
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UserDto user)
        {
            await userService.UpdateUserAsync(user);
            return Ok(user);
        }

        #endregion

        #region DeleteUser
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await userService.DeleteUserAsync(id);
            return Ok("Excluido com sucesso.");
        }

        #endregion


    }
}
