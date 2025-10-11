using Aurum_Domain.Entities;
using Aurum_Domain.Interfaces;
using Aurum_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly InfraContext _infraContext;

        public CategoryRepository(InfraContext infraContext)
        {
            _infraContext = infraContext;
        }

        public async Task<List<Category>> GetAllCategories(User user)
        {
            return await _infraContext.Categories
                .AsNoTracking()
                .Where(c => c.UserId == user.Id)
                .ToListAsync();
        }

        public async Task AddOrUpdateCategory(Category category)
        {
            var existingCategory = await _infraContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == category.Id);

            if (existingCategory != null)
            {
                _infraContext.Categories.Update(category);
            }
            else
            {
                await _infraContext.Categories.AddAsync(category);
            }

            await _infraContext.SaveChangesAsync();
        }

        public async Task DeleteCategory(Category category)
        {
            _infraContext.Categories.Attach(category);
            _infraContext.Categories.Remove(category);
            await _infraContext.SaveChangesAsync();
        }
    }
}
