using Aurum_Application.DTOs;
using Aurum_Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.Service
{
    internal class WalletService : IWalletService
    {
        Task IWalletService.DeleteWalletAsync(Guid userId, Guid walletId)
        {
            throw new NotImplementedException();
        }

        Task<List<WalletDto>> IWalletService.GetAllWalletsAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        Task<WalletDto> IWalletService.GetWalletAsync(Guid userId, Guid walletId)
        {
            throw new NotImplementedException();
        }
    }
}
