using Aurum.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category> GetCategory(Guid guid);
        Task<List<Category>> GetAllCategories();
        Task<Category> CreateCategory(Category category);
        Task<Category> UpdateCategory(Category category);

    }
}
