using Aurum.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Interfaces
{
    public interface IWalletRepository
    {
        Task<IReadOnlyList<Wallet>> GetAllWallets(Guid UserId);
        Task<Wallet> GetWalletById(Guid WalletId, Guid UserId);
        Task<Wallet> CreateWallet(Wallet wallet);
        Task<Wallet> UpdateWallet(Wallet wallet, Guid UserId);
        Task<bool> DeleteWallet(Guid WalletId, Guid UserId);
    }
}
