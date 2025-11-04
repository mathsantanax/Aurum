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
    public class PrivateWalletService
    {
        private readonly IPrivateWalletRepository privateWalletRepository;
        public PrivateWalletService(IPrivateWalletRepository _privateWallet)
        {
            this.privateWalletRepository = _privateWallet;
        }

        public async Task AddWallet(PrivateWalletDTO walletDTO)
        {
            try
            {
                if (walletDTO == null)
                    throw new ArgumentNullException("Inválido");
                if (walletDTO.user == null)
                    throw new ArgumentNullException("Inválido");

                var privateWallet = new PrivateWallet();
                var userWallet = new User();

                userWallet.UpdateUser(walletDTO.user.Guid, walletDTO.user.Name, walletDTO.user.Email, walletDTO.user.PhoneNumber);

                privateWallet.CriarCarteira(walletDTO.NameWallet, userWallet);

                await privateWalletRepository.CriarCarteiraPrivada(privateWallet);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task<PrivateWallet> GetWalletByGuid(PrivateWalletDTO walletDTO)
        {
            try
            {
                if (walletDTO == null)
                    throw new ArgumentNullException("Inválido");

                var wallet = new PrivateWallet();
                var user = new User();

                user.UpdateUser(walletDTO.user.Guid, walletDTO.user.Name, walletDTO.user.Email, walletDTO.user.PhoneNumber);

                wallet.ObterCarteiraPrivada(walletDTO.guid, walletDTO.NameWallet, user);
                var walletData = await privateWalletRepository.ObterCarteiraPrivadaPorGuid(wallet);
                if (walletData == null)
                    throw new ArgumentNullException("Carteira não encontrada");
                return wallet;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task DeleteWallet(PrivateWalletDTO walletDTO)
        {
            try
            {
                if (walletDTO == null)
                    throw new ArgumentNullException("Inválido");

                var wallet = new PrivateWallet();
                var user = new User();
                user.UpdateUser(walletDTO.user.Guid, walletDTO.user.Name, walletDTO.user.Email, walletDTO.user.PhoneNumber);
                wallet.ObterCarteiraPrivada(walletDTO.guid, walletDTO.NameWallet, user);
                await privateWalletRepository.DeletarCarteira(wallet);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

        }
    }

}
