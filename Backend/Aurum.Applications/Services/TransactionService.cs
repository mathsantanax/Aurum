using Aurum.Applications.DTOs;
using Aurum.Applications.DTOs.Enums;
using Aurum.Domain.Entities;
using Aurum.Domain.Entities.Enums;
using Aurum.Domain.Interfaces;


namespace Aurum.Applications.Services
{
    public class TransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        public TransactionService(ITransactionRepository transactionRepository, IWalletRepository walletRepository)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
        }

        public async Task AddTransactionAsync(TransactionDTO request, Guid userGuid)
        {
            try
            {
                TransactionFlow fluxo = request.type switch
                {
                    TransactionFlowDTO.Expense => TransactionFlow.Expense,
                    TransactionFlowDTO.Income => TransactionFlow.Income,
                    TransactionFlowDTO.Transfer => TransactionFlow.Transfer,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var transaction = new Transaction(
                    walletId: request.WalletGuid,
                    categoryId: request.CategoryGuid,
                    amount: request.Amount,
                    description: request.Description,
                    type: fluxo,
                    userGuid: userGuid
                );

                var wallet = await _walletRepository.GetWalletByGuid(request.WalletGuid);

                wallet.AddTransaction(transaction);


                await _transactionRepository.AddAsync(transaction);
                await _walletRepository.UpdateWallet(wallet);
            }
            catch (Exception ex)
            {
                 throw;
            }
        }
    }
}
