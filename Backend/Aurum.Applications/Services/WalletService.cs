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

        public async Task<Wallet> GetWalletById(Guid WalletId, Guid UserId)
        {
            return await _walletRepository.GetWalletById(WalletId, UserId);
        }

        public async Task<Wallet> UpdateNameWallet(WalletDTO request, Guid walletId, Guid userId)
        {
            var walletResult = await _walletRepository.GetWalletById(walletId, userId);

            walletResult.UpdateName(request.Name, userId);

            return await _walletRepository.UpdateNameWallet(walletResult, userId);
        }

    }
}
