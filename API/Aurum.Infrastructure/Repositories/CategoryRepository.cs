using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AurumDbContext _context;

        public CategoryRepository(AurumDbContext context)
        {
            this._context = context;
        }

        public async Task<Category> CreateCategory(Category category)
        {
            await _context.Category.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<List<Category>> GetAllCategories()
        {
            return await _context.Category.ToListAsync();
        }

        public async Task<Category> GetCategory(Guid guid)
        {
            var category = await _context.Category.FirstOrDefaultAsync(c => c.Guid == guid);
            if (category == null)
            {
                throw new Exception("Category not found");
            }
            return category;
        }

        public async Task<Category> UpdateCategory(Category category)
        {
            _context.Category.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }
    }
}
