using Aurum.Application.DTOs;
using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;


namespace Aurum.Application.Services
{
    public class PrivateWalletService
    {
        private readonly IPrivateWalletRepositories privateWalletRepositories;
        private readonly IUserRepository _userRepository;
        public PrivateWalletService(IPrivateWalletRepositories privateWalletRepositories, IUserRepository userRepository)
        {
            this.privateWalletRepositories = privateWalletRepositories;
            this._userRepository = userRepository;
        }

        public async Task<PrivateWallet> CreatePrivateWallet(WalletDto request, Guid guid)
        {
            var user = await _userRepository.GetUser(guid);

            PrivateWallet privateWallet = new PrivateWallet();
            privateWallet.CriarCarteira(request.Name, user);

            var wallet = await privateWalletRepositories.Add(privateWallet);
            wallet.SetNullOwner();
            return wallet;
        }

        public async Task<IEnumerable<PrivateWallet>> GetAllPrivateWalletAsync(Guid ownerGuid)
        {
            var wallets = await privateWalletRepositories.GetAll(ownerGuid);
            foreach (var wallet in wallets)
            {
                wallet.SetNullOwner();
            }
            return wallets;
        }

        public async Task<PrivateWallet> GetPrivateWalletByIdAsync(WalletDto wallet, Guid ownerGuid)
        {
            var foundWallet = await privateWalletRepositories.GetById(wallet.Id, ownerGuid);
            foundWallet.SetNullOwner();
            return foundWallet;
        }

        public async Task<PrivateWallet> UpdatePrivateWalletAsync(WalletDto wallet, Guid ownerGuid)
        {
            var existingWallet = await privateWalletRepositories.GetById(wallet.Id, ownerGuid);
            existingWallet.AtualizarCarteira(wallet.Name);
            var resultWallet = await privateWalletRepositories.Update(existingWallet);
            resultWallet.SetNullOwner();
            return resultWallet;
        }

        public async Task<bool> DeletePrivateWalletAsync(WalletDto wallet, Guid ownerGuid)
        {
            var existingWallet = await privateWalletRepositories.GetById(wallet.Id, ownerGuid);
            return await privateWalletRepositories.Delete(existingWallet);
        }
    }
}
