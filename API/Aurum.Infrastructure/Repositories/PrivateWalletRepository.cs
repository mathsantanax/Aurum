using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Infrastructure.Repositories
{
    public class PrivateWalletRepository : IPrivateWalletRepositories
    {
        private readonly AurumDbContext _context;
        public PrivateWalletRepository(AurumDbContext context)
        {
            this._context = context;
        }

        public async Task<PrivateWallet> Add(PrivateWallet entity)
        {
            await _context.Wallets.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> Delete(PrivateWallet entity)
        {
          var result =  await _context.Wallets
                .OfType<PrivateWallet>()
                .Where(w => w.Guid == entity.Guid && w.OwnerGuid == entity.OwnerGuid)
                .ExecuteDeleteAsync();
            return result > 0;
        }

        public async Task<IEnumerable<PrivateWallet>> GetAll(Guid id)
        {
            return await _context.Wallets
                .OfType<PrivateWallet>()
                .Where(w => w.OwnerGuid == id)
                .ToListAsync();
        }

        public async Task<PrivateWallet> GetById(Guid entity, Guid ownerGuid)
        {
            var wallet = await _context.Wallets
                .OfType<PrivateWallet>()
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.Guid == entity && w.OwnerGuid == ownerGuid);

            if(wallet == null)
                throw new Exception("Carteira não encontrada.");
            return wallet;
        }

        public async Task<PrivateWallet> Update(PrivateWallet entity)
        {
            var result = await _context.Wallets
                .OfType<PrivateWallet>()
                .Where(w => w.Guid == entity.Guid && w.OwnerGuid == entity.OwnerGuid)
                .ExecuteUpdateAsync(w => w
                    .SetProperty(p => p.Name, entity.Name)
                    .SetProperty(p => p.Amount, entity.Amount)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
                );

            if (result == 0)
                throw new Exception("Falha ao atualizar a carteira.");
            return entity;
        }
    }
}
