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
    internal class TransactionRepository<T> : ITransactionRepository<T> where T : Transaction
    {
        private readonly InfraContext infraContext;
        private readonly DbSet<T> dbSet;

        public TransactionRepository(InfraContext context)
        {
            infraContext = context;
            dbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await infraContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "A entidade não pode ser nula.");

            var existingEntity = await dbSet.FindAsync(entity.Id);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Entidade do tipo {typeof(T).Name} com Id '{entity.Id}' não encontrada.");

            dbSet.Remove(existingEntity);
            await infraContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Wallet wallet, DateTime? startDate = null, DateTime? endDate = null)
        {
            IQueryable<T> query = dbSet;

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(x => x.Date >= startDate.Value && x.Date <= endDate.Value);
            }

            return await query.ToListAsync();
        }
    }
}
