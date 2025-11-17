using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> Add(T entity);
        Task<T> GetById(Guid id, Guid ownerGuid);
        Task<IEnumerable<T>> GetAll(Guid id);
        Task<T> Update(T entity);
        Task<bool> Delete(T entity);
    }
}
