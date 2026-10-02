using Aurum.Infrastructure.Persistence;
using Aurum.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AurumDbContext _context;

        public UnitOfWork(AurumDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CommitAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task RollbackAsync()
        {
            var entries = _context
                        .ChangeTracker
                        .Entries()
                        .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted);
                    foreach (var entry in entries)
                    {
                        switch (entry.State)
                        {
                            case EntityState.Added:
                                entry.State = EntityState.Detached;
                                break;
                            case EntityState.Modified:
                            case EntityState.Deleted:
                                await entry.ReloadAsync();
                                break;
                        }
                    }
        }
    }
}
