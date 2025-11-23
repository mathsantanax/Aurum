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
    public class SharedWalletRepository : ISharedWalletRepository
    {
        private readonly AurumDbContext _context;

        public SharedWalletRepository(AurumDbContext context)
        {
            this._context = context;
        }

        public async Task<SharedWallet> Add(SharedWallet entity)
        {
            await _context.Wallets.AddAsync(entity);
            await _context.SaveChangesAsync();

            entity.Members = new List<Members>();

            foreach (var member in entity.Members)
            {
                entity.Members.Add(new Members(member.UserGuid, entity.Guid, member.WalletRole));
            }

            return entity;
        }

        public async Task<bool> Delete(SharedWallet entity)
        {
            var result = await _context.Wallets
                .OfType<SharedWallet>()
                .Where(w => w.Guid == entity.Guid && w.OwnerGuide == entity.OwnerGuide)
                .ExecuteDeleteAsync();
            return result > 0;
        }

        public async Task<IEnumerable<SharedWallet>> GetAll(Guid id)
        {
            return await _context.Wallets
                .OfType<SharedWallet>()
                .Include(sw => sw.Members)
                .Include(sw => sw.Transactions)
                .Where(sw => sw.OwnerGuide == id || sw.Members.Any(m => m.UserGuid == id))
                .ToListAsync();
        }

        public Task<IEnumerable<SharedWallet>> GetAllWalletMembers(Guid memberGuid)
        {
            return Task.FromResult(_context.Wallets
                .OfType<SharedWallet>()
                .AsEnumerable());
        }

        public async Task<SharedWallet> GetById(Guid id, Guid ownerGuid)
        {
            var wallet = await _context.Wallets
                .OfType<SharedWallet>()
                .Where(w => w.Members.Any(m => m.UserGuid == ownerGuid) && w.Guid == id)
                .FirstOrDefaultAsync();

            if (wallet == null)
                throw new ArgumentException("Carteira não econtrada!");

            return wallet;
        }

        public Task<SharedWallet> GetByIdMenbers(Guid id, Guid memberGuid)
        {
            var wallets = Task.FromResult(_context.Wallets
                .OfType<SharedWallet>()
                .Include(sw => sw.Members)
                .Include(sw => sw.Transactions)
                .FirstOrDefault(w => w.Guid == id && w.Members.Any(m => m.UserGuid == memberGuid)));

            if (wallets == null)
                throw new ArgumentException("Carteira não econtrada!");
            
            return wallets;
        }

        public Task<SharedWallet> Update(SharedWallet entity)
        {
            _context.Set<SharedWallet>().Update(entity);
            return Task.FromResult(entity);
        }
    }
}
