using Aurum.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Domain.Interfaces
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        Task<IReadOnlyList<TEntity>> GetAllAsync();

        Task<TEntity?> GetByIdAsync(Guid id);

        Task AddAsync(TEntity entity);

        void Update(TEntity entity);

        void Remove(TEntity entity);
        Task<bool> ExistsAsync(Guid id);
    }
}
