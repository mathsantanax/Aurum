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
    public class TransactionRepository<T> : ITransactionRepository<T> where T : Transaction
    {
        private readonly InfraContext _infraContext;
        private readonly DbSet<T> _dbSet;

        public TransactionRepository(InfraContext context)
        {
            _infraContext = context;
            _dbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "A entidade não pode ser nula.");

            await _dbSet.AddAsync(entity);
            await _infraContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "A entidade não pode ser nula.");

            var existingEntity = await _dbSet.FindAsync(entity.Id);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Entidade do tipo {typeof(T).Name} com Id '{entity.Id}' não encontrada.");

            _dbSet.Remove(existingEntity);
            await _infraContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            Wallet? wallet = null,
            SharedWallet? sharedWallet = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (wallet == null && sharedWallet == null)
                throw new ArgumentException("É necessário informar pelo menos uma carteira (Wallet ou SharedWallet).");

            IQueryable<T> query = _dbSet.AsQueryable();

            if (wallet != null)
                query = query.Where(x => x.WalletId == wallet.Id);

            if (sharedWallet != null)
                query = query.Where(x => x.SharedWalletId == sharedWallet.Id);

            if (startDate.HasValue)
                query = query.Where(x => x.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.CreatedAt <= endDate.Value);

            return await query.AsNoTracking().ToListAsync();
        }
    }
}
