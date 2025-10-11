using Aurum_Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDTO>> GetAllCategoriesAsync(Guid userId);
        Task AddOrUpdateCategoryAsync(CategoryDTO category);
        Task DeleteCategoryAsync(Guid categoryId);
    }
}
