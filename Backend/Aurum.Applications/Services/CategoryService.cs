using Aurum.Applications.DTOs;
using Aurum.Applications.DTOs.Enums;
using Aurum.Domain.Entities;
using Aurum.Domain.Entities.Enums;
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
            // Mapear CategoryDTO para Category
            TransactionFlow fluxo = category.IsIncome switch
            {
                TransactionFlowDTO.Expense => TransactionFlow.Expense,
                TransactionFlowDTO.Income => TransactionFlow.Income,
                TransactionFlowDTO.Transfer => TransactionFlow.Transfer,
                _ => throw new ArgumentOutOfRangeException()
            };
            // Criar nova categoria
            var novaCategoria = new Category(userGuid, category.NameCagegory, category.DescriptionCagegory, fluxo);
            await _categoryRepository.AddAsync(novaCategoria);// Salvar no repositório

            return category;
        }

        public async Task<IEnumerable<CategoryDTO>> GetCategoriesByUserGuidAsync(Guid userGuid)
        {
            var categories = await _categoryRepository.GetByUserGuidAsync(userGuid);
            // Mapear Category para CategoryDTO
            var categoryDTOs = categories.Select(c => new CategoryDTO
            {
                NameCagegory = c.Name,
                DescriptionCagegory = c.Description,
                IsIncome = c.Flow switch
                {
                    TransactionFlow.Expense => TransactionFlowDTO.Expense,
                    TransactionFlow.Income => TransactionFlowDTO.Income,
                    TransactionFlow.Transfer => TransactionFlowDTO.Transfer,
                    _ => throw new ArgumentOutOfRangeException()
                }
            });
            return categoryDTOs;
        }
    }
}
