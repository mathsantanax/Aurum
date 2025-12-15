using Aurum.Applications.DTOs;
using Aurum.Applications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aurum.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;
        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] CategoryDTO category)
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(user) || !Guid.TryParse(user, out var userGuid))
                // Se o token de autenticação não fornecer um UserGuid válido
                return Unauthorized("Não foi possível identificar o usuário logado.");

            // Adiciona a nova categoria para o usuário autenticado
            return Ok(await _categoryService.AddCategoryAsync(userGuid, category));
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(user) || !Guid.TryParse(user, out var userGuid))
                // Se o token de autenticação não fornecer um UserGuid válido
                return Unauthorized("Não foi possível identificar o usuário logado.");
            // Recupera as categorias do usuário autenticado
            var categories = await _categoryService.GetCategoriesByUserGuidAsync(userGuid);
            return Ok(categories);
        }
    }
}
