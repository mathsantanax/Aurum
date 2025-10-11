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
    public class WalletRepository : IWalletRepository
    {
        private readonly InfraContext _infraContext;

        public WalletRepository(InfraContext infraContext)
        {
            _infraContext = infraContext;
        }

        public async Task DeleteWallet(User user, Wallet wallet)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "Usuário não pode ser nulo.");
            if (wallet == null)
                throw new ArgumentNullException(nameof(wallet), "Carteira não pode ser nula.");

            try
            {
                var existingWallet = await _infraContext.Wallets
                    .FirstOrDefaultAsync(w => w.Id == wallet.Id && w.UserId == user.Id);

                if (existingWallet == null)
                    throw new KeyNotFoundException($"Carteira {wallet.Id} não encontrada para o usuário {user.Id}.");

                _infraContext.Wallets.Remove(existingWallet);
                await _infraContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Erro ao excluir a carteira no banco de dados.", ex);
            }
        }

        public async Task<List<Wallet>> GetAllWallets(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "Usuário não pode ser nulo.");
            if (user.Id == Guid.Empty)
                throw new ArgumentException("O Id do usuário não pode ser vazio.", nameof(user));

            try
            {
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                return await _infraContext.Wallets
                    .Where(w => w.UserId == user.Id)
                    .Include(w => w.Transactions
                        .Where(t => t.CreatedAt.Month == currentMonth && t.CreatedAt.Year == currentYear))
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao buscar todas as carteiras do usuário.", ex);
            }
        }
    }
}
