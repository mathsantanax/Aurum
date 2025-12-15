using Aurum.Applications.Exceptions;
using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Infrastructure.Repositories
{
    public class CategoryRepository(AppDbContext context) : ICategoryRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Category> AddAsync(Category category)
        {
            try
            {
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
                return category;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("violates unique constraint") == true)
                {
                    throw new ValidationException("O nome do item já existe. Escolha outro nome.");
                }

                // Se for outro erro de banco, relança como um erro 500 de aplicação.
                throw new AppException($"Erro ao salvar o item no banco de dados. {ex.Message}", ex.HResult);
            }
        }

        public async Task<IEnumerable<Category>> GetByUserGuidAsync(Guid userGuid)
        {
            try
            {
                return await _context.Categories
                    .Where(c => c.UserGuid == userGuid)
                    .AsSplitQuery()
                    .ToListAsync()
                    .ContinueWith(t => (IEnumerable<Category>)t.Result);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("violates unique constraint") == true)
                {
                    throw new ValidationException("O nome do item já existe. Escolha outro nome.");
                }

                // Se for outro erro de banco, relança como um erro 500 de aplicação.
                throw new AppException($"Erro ao salvar o item no banco de dados. {ex.Message}", ex.HResult);
            }
        }
    }

}
