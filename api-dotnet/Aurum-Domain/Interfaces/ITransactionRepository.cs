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
        Task AddAsync(T entity); //adicionar transaction (Income ou Cost)
        Task<IEnumerable<T>> GetAllAsync(Wallet wallet, DateTime? startDate = null, DateTime? endDate = null); // Listar Transaction (Income e Cost)
        Task DeleteAsync(T entity); // Deletar Transaction (Income ou Cost)
    }
}
