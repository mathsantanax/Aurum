using Aurum.Applications.DTOs;
using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Applications.Services
{

    public class WalletService(IWalletRepository walletRepository)
    {
        private readonly IWalletRepository _walletRepository = walletRepository;

        public async Task<Wallet> CreateWalletAsync(WalletDTO request, Guid UserGuid)
        {
            var wallet = new Wallet(UserGuid, request.Name);
            return await _walletRepository.CreateWallet(wallet);
        }

        public async Task<IReadOnlyList<Wallet>> GetAllWalletsAsync(Guid userId)
        {
            return await _walletRepository.GetAllWallets(userId);
        }

    }
}
