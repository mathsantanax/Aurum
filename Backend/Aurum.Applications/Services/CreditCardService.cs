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

        // Cria um novo cartão de crédito para um usuário em uma carteira específica
        public async Task<CreditCartDTO> CreateCreditCard(Guid UserGuid, Guid WalletGuid, CreditCartDTO request)
        {
            // Cria um novo cartão de crédito
            var creditCard = new CreditCard(
                UserGuid,
                walletId: WalletGuid,
                request.Name,
                request.CreditLimit,
                request.ClosingDay,
                request.DueDay
            );

            // Salva o cartão de crédito no repositório
            var credit = await _creditCardRepository.CreateCreditCard(creditCard);

            Console.WriteLine("\n\n\n\nCredit Card created with ID: " + creditCard.Id + "\n\n\n\n");

            // Retorna os dados do cartão de crédito criado
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
