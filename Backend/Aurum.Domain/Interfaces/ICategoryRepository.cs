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
        Task<Category> AddAsync(Category category);
        Task<IEnumerable<Category>> GetByUserGuidAsync(Guid userGuid);
    }
}
