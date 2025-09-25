using Aurum_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Interfaces
{
    public interface ISharedWalletRepository
    {
        Task<SharedWallet> GetSharedWallet(Guid walletId, Guid userId);
        Task<List<SharedWallet>> GetAllSharedWallets(User user);
        Task DeleteSharedWallet(User user, SharedWallet sharedWallet);
    }
}
