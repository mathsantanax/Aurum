using Aurum.Application.DTOs;
using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Application.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Category> CreateCategory(CategoryDTO request)
        {
            if (request == null)
                throw new ArgumentNullException("Categoria não pode estar nula.");

            Category category1 = new Category();
            category1.AddCategoria(request.Title, request.Description);

            await _categoryRepository.CreateCategory(category1);
            return category1;
        }

        public async Task<List<Category>> GetAllCategories()
        {
            return await _categoryRepository.GetAllCategories();
        }

        public async Task<Category> GetCategory(Guid guid)
        {
            return await _categoryRepository.GetCategory(guid);
        }

        public async Task<Category> UpdateCategory(CategoryDTO request, Guid guid)
        {
            var category = await _categoryRepository.GetCategory(guid);
            if (category == null)
                throw new Exception("Categoria não encontrada.");
            category.UpdateCategoria(request.Title, request.Description);
            await _categoryRepository.UpdateCategory(category);
            return category;
        }
    }
}
