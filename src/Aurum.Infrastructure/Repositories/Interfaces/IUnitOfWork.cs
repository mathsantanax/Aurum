using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Infrastructure.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        Task<bool> CommitAsync();
        Task RollbackAsync();
    }
}
