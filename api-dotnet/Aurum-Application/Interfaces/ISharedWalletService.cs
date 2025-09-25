using Aurum_Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.Interfaces
{
    public interface ISharedWalletService
    {
        Task<SharedWalletDTO> GetSharedWalletAsync(Guid userId, Guid sharedWalletId);
        Task<List<SharedWalletDTO>> GetAllSharedWalletsAsync(Guid userId);
        Task DeleteSharedWalletAsync(Guid userId, Guid sharedWalletId);
    }
}
