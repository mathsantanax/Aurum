using Aurum_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUser(Guid guid); // Busca Usuario 
        Task AddAsync (User user); // Adiciona Usuario 
        Task UpdateAsync (User user); // Atualisa os dados do Usuario
        Task DeleteAsync (User user); // Deleta os dados do usuario
    }
}
