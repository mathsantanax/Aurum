using Aurum.Application.Interfaces.Auth;
using Aurum.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace Aurum.Api.Controllers.AuthControllers
{
    [ApiController]
    [Authorize]
    [EnableRateLimiting("SessionRateLimit")]
    public class LogoutController : ControllerBase
    {
        private readonly SignInManager<AurumUser> _signInManager;
        private readonly UserManager<AurumUser> _userManager;
        private readonly ICurrentUser _currentUser;

        public LogoutController(SignInManager<AurumUser> signInManager, UserManager<AurumUser> userManager, ICurrentUser currentUser)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _currentUser = currentUser;
        }

        [HttpPost("api/auth/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }

        [HttpGet("api/auth/me")]
        public async Task<IActionResult> Me(
        CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(
                _currentUser.UserId.ToString());

            if (user is null)
                return Unauthorized();

            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                profileComplete = IsProfileComplete(user)
            });
        }

        [HttpPut("api/auth/profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileRequest request)
        {
            var user = await _userManager.FindByIdAsync(
                _currentUser.UserId.ToString());

            if (user is null)
                return Unauthorized();

            user.FullName = request.FullName.Trim();
            user.PhoneNumber = request.PhoneNumber.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return ValidationProblem(ModelState);
            }

            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                profileComplete = IsProfileComplete(user)
            });
        }

        private static bool IsProfileComplete(AurumUser user) =>
            !string.IsNullOrWhiteSpace(user.FullName) &&
            !string.IsNullOrWhiteSpace(user.PhoneNumber);

        public sealed class UpdateProfileRequest
        {
            [Required]
            [StringLength(250, MinimumLength = 2)]
            public string FullName { get; init; } = string.Empty;

            [Required]
            [Phone]
            [StringLength(32)]
            public string PhoneNumber { get; init; } = string.Empty;
        }
    }
}
