using Aurum.Application.DTOs;
using Aurum.Application.Services;
using Aurum.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly AuthService authService;

        public AuthController(AuthService authService)
        {
            this.authService = authService;
        }

        /// Rota de login para autenticação e emissão de um JWT.
        [HttpPost("login")]
        // O LoginDTO deve conter Email e Password
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            // 1. Validação do DTO (o framework já faz a validação básica, mas a lógica de domínio é no serviço)
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest(new { message = "E-mail e senha são obrigatórios." });
            }

            // 2. Chama o serviço para validar as credenciais e gerar o token
            var token = await authService.AuthenticateAndGenerateToken(loginDto.Email, loginDto.Password);

            if (string.IsNullOrEmpty(token))
            {
                // Retorna 401 Unauthorized se as credenciais forem inválidas.
                return Unauthorized(new { message = "E-mail ou senha inválidos." });
            }

            // 3. Sucesso: Retorna o token JWT.
            return Ok(new
            {
                token = token,
                message = "Autenticação bem-sucedida."
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenRefreshDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { message = "Tokens são obrigatórios." });
            }

            var (newJwt, newRefreshToken) = await authService.RefreshTokens(request.Token, request.RefreshToken);

            if (string.IsNullOrEmpty(newJwt))
            {
                // Falha na atualização (Refresh Token inválido/expirado)
                return Unauthorized(new { message = "Falha na atualização. Requer novo login." });
            }

            return Ok(new
            {
                jwtToken = newJwt,
                refreshToken = newRefreshToken,
                message = "Tokens atualizados com sucesso."
            });
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (registerDto == null ||
                string.IsNullOrEmpty(registerDto.Email) ||
                string.IsNullOrEmpty(registerDto.FullName) ||
                string.IsNullOrEmpty(registerDto.PhoneNumber) ||
                string.IsNullOrEmpty(registerDto.Password) ||
                string.IsNullOrEmpty(registerDto.ConfirmedPassword))
            {
                return BadRequest(new { message = "Todos os campos são obrigatórios." });
            }
            try
            {
                var user = await authService.RegisterUser(registerDto);
                return Ok(new
                {
                    userId = user.Id,
                    message = "Usuário registrado com sucesso."
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
