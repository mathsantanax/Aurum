using Aurum_Application.DTOs;
using Aurum_Domain.Entities;
using Aurum_Domain.Interfaces;
using Aurum_Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.Service
{
    public class TransactionService<T> where T : Transaction
    {
        private readonly ITransactionRepository<T> _repository;

        public TransactionService(ITransactionRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TransactionDTO>> GetAllAsync(WalletDto walletDto, DateTime? start = null, DateTime? end = null)
        {
            var transactions = await _repository.GetAllAsync(walletDto, start, end);

            return transactions.Select(t => new TransactionDTO
            {
                guid = t.Guid,

                Description = t.Description!,
                Value = t.Value.Amount,
                Date = t.Date,
                CategoryId = t.CategoryId,
                WalletId = t.WalletId
            });
        }

        public async Task AddAsync(TransactionDTO dto)
        {
            var transaction = (T)Activator.CreateInstance(typeof(T), dto.Description, new Money(dto.Value), new Category(dto.CategoryId, "Temp"))!;

            await _repository.AddAsync(transaction);
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            await _repository.DeleteByIdAsync(id);
        }


    }
}
