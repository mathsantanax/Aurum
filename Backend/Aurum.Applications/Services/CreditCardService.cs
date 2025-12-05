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
    public class CreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IWalletRepository _walletRepository;
        public CreditCardService(
            ICreditCardRepository creditCardRepository,
            IWalletRepository walletRepository)
        {
            _creditCardRepository = creditCardRepository;
            _walletRepository = walletRepository;
        }

        public async Task<CreditCartDTO> CreateCreditCard(Guid UserGuid, Guid WalletGuid, CreditCartDTO request)
        {
            var wallet = await _walletRepository.GetWalletByIdAsync(WalletGuid);

            if (wallet == null)
                throw new Exception("Carteira não cadastrada!");

            var creditCard = new CreditCard(
                UserGuid,
                wallet.Id,
                request.Name,
                request.CreditLimit,
                request.ClosingDay,
                request.DueDay
            );

            var credit = await _creditCardRepository.CreateCreditCard(creditCard);
            Console.WriteLine(credit);

            return new CreditCartDTO 
            {
                Name = creditCard.Name!,
                CreditLimit = creditCard.CreditLimit,
                ClosingDay = creditCard.ClosingDay,
                DueDay = creditCard.DueDay
            };
        }

        public async Task<CreditCard> GetCreditCard(Guid CreditCardGuid)
        {
            var creditCard = await _creditCardRepository.GetCreditCard(CreditCardGuid);
            if (creditCard == null)
                throw new Exception("Cartão de crédito não cadastrado!");
            return creditCard;
        }

    }
}
