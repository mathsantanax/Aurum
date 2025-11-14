using Aurum.Api.Service;
using Aurum.Application.DTOs;
using Aurum.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService authService;

        public AuthController(UserService userService, AuthService service)
        {
            authService = service;
            _userService = userService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody]RegisterDTO request)
        {
            try
            {
                await _userService.RegisterUser(
                    new UserDTO
                    {
                        Name = request.FullName!,
                        Email = request.Email!,
                        PhoneNumber = request.PhoneNumber!,
                        PassWord = request.Password!,
                        ConfirmedPassword = request.ConfirmedPassword!
                    });

                return Ok("Usuário registrado com sucesso!");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                    return BadRequest("Email e senha são obrigatórios.");

                var login = await _userService.LoginUser(
                    new UserDTO
                    {
                        Email = request.Email!,
                        PassWord = request.Password!
                    });

                Console.WriteLine(login);

                var token = authService.GenerateJwtToken(login);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
