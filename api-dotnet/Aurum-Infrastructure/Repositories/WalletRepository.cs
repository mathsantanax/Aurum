using Aurum_Domain.Entities;
using Aurum_Domain.Interfaces;
using Aurum_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Infrastructure.Repositories
{
    internal class WalletRepository : IWalletRepository
    {
        private readonly InfraContext infraContext;

        public WalletRepository(InfraContext _infraContext)
        {
            infraContext = _infraContext;
        }
        public async Task DeleteWallet(User user, Wallet wallet)
        {
            var existingWallet = await infraContext.Wallet.AsNoTracking().FirstOrDefaultAsync(w => w.Id.Equals(wallet.Id) && w.UserId.Equals(user.Id));
            if (existingWallet != null)
            {
                infraContext.Wallet.Remove(existingWallet);
                await infraContext.SaveChangesAsync();
            }
        }

        // buscando todas as carteiras com as receitas/gastos do mês
        public async Task<List<Wallet>> GetAllWallets(User user)
        {
            return await infraContext.Wallet.AsNoTracking().Where(w => w.UserId.Equals(user.Id))
                .Include(w => w.Costs.Where(c => c.Date.Month.Equals(DateTime.Now.Month)))
                .Include(w => w.Incomes.Where(i => i.Date.Month.Equals(DateTime.Now.Month)))
                .ToListAsync();
        }

        public async Task<Wallet> GetWallet(User user, Wallet wallet)
        {
            return await infraContext.Wallet
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId.Equals(user.Id) && w.Id.Equals(wallet.Id));
        }
    }
}
