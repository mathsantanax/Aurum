using Aurum.Application.DTOs;
using Aurum.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aurum.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService categoryService;
        public CategoryController(CategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        [Authorize]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllCategoriesAsync()
        {
            var nameIdentifier = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (nameIdentifier == Guid.Empty)
                return Unauthorized(new { message = "Usuário não autenticado." });

            var categories = await categoryService.GetAllCategories();
            return Ok(categories);
        }

        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDTO request)
        {
            var nameIdentifier = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (nameIdentifier == Guid.Empty)
                return Unauthorized(new { message = "Usuário não autenticado." });

            if (string.IsNullOrEmpty(request.Title))
            {
                return BadRequest(new { message = "Título da categoria é obrigatório." });
            }
            var category = await categoryService.CreateCategory(request);
            return Ok(category);
        }

        [Authorize]
        [HttpGet("GetById/{guid}")]
        public async Task<IActionResult> GetCategoryByIdAsync([FromRoute] Guid guid)
        {
            var nameIdentifier = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (nameIdentifier == Guid.Empty)
                return Unauthorized(new { message = "Usuário não autenticado." });

            var category = await categoryService.GetCategory(guid);
            if (category == null)
            {
                return NotFound(new { message = "Categoria não encontrada." });
            }
            return Ok(category);
        }

        [Authorize]
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryDTO request)
        {
            var nameIdentifier = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (nameIdentifier == Guid.Empty)
                return Unauthorized(new { message = "Usuário não autenticado." });

            if (string.IsNullOrEmpty(request.Title))
            {
                return BadRequest(new { message = "Título da categoria é obrigatório." });
            }
            try
            {
                var category = await categoryService.UpdateCategory(request, nameIdentifier);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
