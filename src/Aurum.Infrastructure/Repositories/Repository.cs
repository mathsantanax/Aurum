using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Infrastructure.Repositories
{
    public class Repository<Entity> : IRepository<Entity> where Entity : BaseEntity
    {
        protected readonly AurumDbContext _context;
        protected readonly DbSet<Entity> _dbSet;

        public Repository(AurumDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Entity>();
        }

        public async Task AddAsync(Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            return await _dbSet.AnyAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<Entity>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<Entity?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        }

        public void Remove(Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
        }

        public void Update(Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
        }
    }
}
