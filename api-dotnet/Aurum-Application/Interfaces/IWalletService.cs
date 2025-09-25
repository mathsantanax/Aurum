using Aurum_Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> GetWalletAsync(Guid userId, Guid walletId);
        Task<List<WalletDto>> GetAllWalletsAsync(Guid userId);
        Task DeleteWalletAsync(Guid userId, Guid walletId);
    }
}
