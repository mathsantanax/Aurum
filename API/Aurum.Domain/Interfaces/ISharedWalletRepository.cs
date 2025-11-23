using Aurum.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Interfaces
{
    public interface ISharedWalletRepository : IRepository<SharedWallet>
    {
        Task<IEnumerable<SharedWallet>> GetAllWalletMembers(Guid memberGuid);
        Task<SharedWallet> GetByIdMenbers(Guid id, Guid memberGuid);
    }
}
