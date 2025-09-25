using Aurum_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Interfaces
{
    public interface ITransactionRepository<T> where T : Transaction
    {
        Task AddAsync(T entity); // adicionar Income ou Cost
        Task DeleteAsync(T entity); // deletar Income ou Cost

        Task<IEnumerable<T>> GetAllAsync(
            Wallet? wallet = null,
            SharedWallet? sharedWallet = null,
            DateTime? startDate = null,
            DateTime? endDate = null
        ); // Listar por Wallet OU SharedWallet
    }
}
