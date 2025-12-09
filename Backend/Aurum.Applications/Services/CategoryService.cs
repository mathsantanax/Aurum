using Aurum.Applications.DTOs;
using Aurum.Applications.DTOs.Enums;
using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Applications.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryDTO> AddCategoryAsync(Guid userGuid, CategoryDTO category)
        {


            var category = new Category 
            {
                UserGuid = userGuid,
                Name = category.NameCagegory,
                Description = category.DescriptionCagegory,
                IsIncome = TransactionFlow{
                    
                }
            };
                await _categoryRepository.AddAsync(category);
        }
    }
}
