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
    internal class CategoryRepository : ICategoryRepository
    {
        private readonly InfraContext infraContext;

        public CategoryRepository(InfraContext infraContext )
        {
            this.infraContext = infraContext;
        }

        public async Task DeleteCategory(Category category)
        {
            var existingCategory = await infraContext.Category.AsNoTracking().FirstAsync(c => c.Id.Equals(category.Id));
            if(existingCategory != null)
            {
                infraContext.Category.Remove(existingCategory);
                await infraContext.SaveChangesAsync();
            }
        }

        public async Task<List<Category>> GetAllCategories(User user)
        {
            return await infraContext.Category
                .AsNoTracking()
                .Where(c => c.UserId.Equals(user.Id))
                .ToListAsync();
        }

        public async Task AddOrUpdatedCategory(Category category)
        {
            var existingCategory = await infraContext.Category.AsNoTracking().FirstAsync(c => c.Id.Equals(category.Id));

            if(existingCategory != null)
            {
                infraContext.Category.Update(category);
                await infraContext.SaveChangesAsync();
            }

            await infraContext.Category.AddAsync(category);
            await infraContext.SaveChangesAsync();
        }
    }
}
