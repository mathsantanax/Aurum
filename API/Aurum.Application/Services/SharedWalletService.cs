using Aurum.Application.DTOs;
using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Application.Services
{
    public class SharedWalletService
    {
        private readonly ISharedWalletRepository _sharedWalletRepository;
        private readonly IUserRepository _userRepository;

        public SharedWalletService(ISharedWalletRepository sharedWalletRepository, IUserRepository userRepository)
        {
            _sharedWalletRepository = sharedWalletRepository;
            _userRepository = userRepository;
        }

        public async Task<object> CreateSharedWallet(WalletDto request, Guid guid)
        {
            var resultUser = await _userRepository.GetUser(guid);

            var wallet = new SharedWallet(request.Name, resultUser);
            await _sharedWalletRepository.Add(wallet);

            var result = new
            {
                Id = wallet.Guid,
                Name = wallet.Name,
                Amount = wallet.Amount,
                CreatedAt = wallet.CreatedAt,
                UpdatedAt = wallet.UpdatedAt,
                Members = wallet.Members.Select(member => new
                {
                    UserGuid = member.UserGuid,
                    UserName = member.User?.fullName,
                    WalletRole = member.WalletRole,
                    JoinedAt = member.JoinedAt
                }).ToList()

            };
            return result;
        }

        public async Task<IEnumerable<object>> GetAllSharedWallets(Guid guid)
        {
            var wallet = await _sharedWalletRepository.GetAll(guid);

            var result = wallet.Select(wallet => new
            {
                Id = wallet.Guid,
                Name = wallet.Name,
                Amount = wallet.Amount,
                CreatedAt = wallet.CreatedAt,
                UpdatedAt = wallet.UpdatedAt,
                Members = wallet.Members.Select(member => new
                {
                    UserGuid = member.UserGuid,
                    UserName = member.User?.fullName,
                    WalletRole = member.WalletRole,
                    JoinedAt = member.JoinedAt
                }).ToList()

            });
            return result;
        }

        public async Task<object?> GetSharedWalletById(Guid walletGuid, Guid ownerGuid)
        {
            var wallet = await _sharedWalletRepository.GetById(walletGuid, ownerGuid);

            var result = new
            {
                Id = wallet.Guid,
                Name = wallet.Name,
                Amount = wallet.Amount,
                CreatedAt = wallet.CreatedAt,
                UpdatedAt = wallet.UpdatedAt,
                Members = wallet.Members.Select(member => new
                {
                    UserGuid = member.UserGuid,
                    UserName = member.User?.fullName,
                    WalletRole = member.WalletRole,
                    JoinedAt = member.JoinedAt
                }).ToList()

            };
            return result;
        }

        public async Task<object> UpdateSharedWallet(WalletDto request, Guid userGuid)
        {
            var walletResult = await _sharedWalletRepository.GetById(request.Id, userGuid);

            walletResult.ChangeNameWallet(request.Name);

            var wallet = await _sharedWalletRepository.Update(walletResult);

            var result = new
            {
                Id = wallet.Guid,
                Name = wallet.Name,
                Amount = wallet.Amount,
                CreatedAt = wallet.CreatedAt,
                UpdatedAt = wallet.UpdatedAt,
                Members = wallet.Members.Select(member => new
                {
                    UserGuid = member.UserGuid,
                    UserName = member.User?.fullName,
                    WalletRole = member.WalletRole,
                    JoinedAt = member.JoinedAt
                }).ToList()

            };
            return result;


        }

    }
}
