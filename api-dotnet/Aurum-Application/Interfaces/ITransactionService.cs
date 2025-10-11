using Aurum_Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.Interfaces
{
    public interface ITransactionService
    {
        Task AddTransactionAsync(TransactionDTO transaction);
        Task<IEnumerable<TransactionDTO>> GetAllTransactionsAsync(Guid walletId, DateTime? startDate = null, DateTime? endDate = null);
        Task DeleteTransactionAsync(Guid transactionId);
    }
}
