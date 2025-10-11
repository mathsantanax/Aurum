using Aurum_Domain.Entities;
using Aurum_Domain.Interfaces;
using Aurum_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Infrastructure.Repositories
{
    public class SharedWalletRepository : ISharedWalletRepository
    {
        private readonly InfraContext _infraContext;

        public SharedWalletRepository(InfraContext infraContext)
        {
            _infraContext = infraContext;
        }

        public async Task AddUser(User user, SharedWallet sharedWallet)
        {
            if(user == null)
                throw new ArgumentNullException(nameof(user), "Usuário não pode ser nulo.");
            if(sharedWallet == null)
                throw new ArgumentNullException(nameof(sharedWallet), "Carteira não pode ser nula.");

            try
            {
                var wallet = await _infraContext.SharedWallets
                    .Include(w => w.Members)
                    .SingleOrDefaultAsync(w => w.Id.Equals(sharedWallet.Id) && w.OwnerId.Equals(sharedWallet.OwnerId));

                wallet.AddMember(user);
                await _infraContext.SaveChangesAsync();
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao Adcionar Usuário.", ex);
            }
        }

        public async Task DeleteSharedWallet(User user, SharedWallet sharedWallet)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "Usuário não pode ser nulo.");
            if (sharedWallet == null)
                throw new ArgumentNullException(nameof(sharedWallet), "Carteira não pode ser nula.");
            if (user.Id == Guid.Empty || sharedWallet.Id == Guid.Empty)
                throw new ArgumentException("O Id do usuário ou da carteira não pode ser vazio.");

            try
            {
                var existingWallet = await _infraContext.SharedWallets
                    .FirstOrDefaultAsync(w => w.Id == sharedWallet.Id && w.OwnerId == user.Id);

                if (existingWallet == null)
                    throw new UnauthorizedAccessException("Somente o proprietário pode excluir esta carteira.");

                _infraContext.SharedWallets.Remove(existingWallet);
                await _infraContext.SaveChangesAsync();
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao excluir a carteira compartilhada.", ex);
            }
        }

        public async Task<List<SharedWallet>> GetAllSharedWallets(User user)
        {
            if(user == null)
                throw new ArgumentNullException(nameof(user), "Usuário não pode ser nulo.");
            if(user.Id == Guid.Empty)
                throw new ArgumentException("O Id do usuário não pode ser vazio.", nameof(user));

            try
            {
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                return await _infraContext.SharedWallets
                    .Where(w => w.Members.Any(u => u.Id == user.Id))
                    .Include(w => w.Transactions
                    .Where(t => t.CreatedAt.Month == currentMonth && t.CreatedAt.Year == currentYear))
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Erro ao buscar todas as carteiras do usuário.", ex);
            }
        }
    }
}
