using Aurum_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Interfaces
{
    public interface IWalletRepository
    {
        Task<List<Wallet>> GetAllWallets(User user); // buscar todas as carteiras
        Task DeleteWallet(User user, Wallet wallet); // deletar uma carteira
    }
}
