using Aurum.Api.Service;
using Aurum.Application.DTOs;
using Aurum.Application.Interfaces;
using Aurum.Application.Services;
using Aurum.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IAuthService authService;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IAuthService authService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult<UserResponse>> Register([FromBody]RegisterRequest request)
        {
            try
            {
                if (request.Password != request.ConfirmPassword)
                    return BadRequest("Passwords do not match");

                var user = new User
                {
                    UserName = request.UserName ?? request.Email,
                    Email = request.Email,
                    FullName = request.FullName
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return Ok(new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    UserName = user.UserName
                });
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
