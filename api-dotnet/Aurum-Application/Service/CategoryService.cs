using Aurum_Application.DTOs;
using Aurum_Application.Interfaces;
using Aurum_Domain.Entities;
using Aurum_Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task AddOrUpdateCategoryAsync(CategoryDTO category)
        {
            var existingCategory = await _categoryRepository.GetById(category.Id);
            if (existingCategory != null)
            {
                await _categoryRepository.AddOrUpdateCategory(new Category(category.Id, category.Description));
            }
            else
            {
                await _categoryRepository.AddOrUpdateCategory(new Category(category.Description));
            }
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            var existingCategory = await _categoryRepository.GetById(categoryId);

            if (existingCategory != null)
            {
                await _categoryRepository.DeleteCategory(existingCategory);
            }
        }

        public async Task<List<CategoryDTO>> GetAllCategoriesAsync(Guid userId)
        {
            var categories = await _categoryRepository.GetAllCategories(userId);
            return categories.Select(c => CategoryDTO.FromEntity(c)).ToList();
        }
    }
}
