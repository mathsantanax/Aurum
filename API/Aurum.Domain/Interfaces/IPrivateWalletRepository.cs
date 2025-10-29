using Aurum.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Interfaces
{
    public interface IPrivateWalletRepository
    {
        Task CriarCarteiraPrivada(PrivateWallet wallet);
        Task DeletarCarteira(Wallet wallet);
    }
}
