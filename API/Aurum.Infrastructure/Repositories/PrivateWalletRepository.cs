using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Infrastructure.Repositories
{
    public class PrivateWalletRepository : IPrivateWalletRepository
    {
        private readonly AurumDbContext _context;

        public PrivateWalletRepository(AurumDbContext context)
        {
            _context = context;
        }

        public async Task CriarCarteiraPrivada(PrivateWallet wallet)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == wallet.User.Id);
                if (user == null)
                    throw new ArgumentException("Usuário não encontrado.");

                var privateWallet = new PrivateWallet();

                privateWallet.CriarCarteira(wallet.Name, user);

                user.Wallets.Add(privateWallet);
                _context.Entry(user).State = EntityState.Modified;

                await _context.PrivateWallets.AddAsync(privateWallet);
                await _context.SaveChangesAsync();
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException("Erro de comunicação com o banco de dados PostgreSQL.\n" + ex.Message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro inesperado \n" + ex.Message);
            }
        }

        public async Task DeletarCarteira(Wallet wallet)
        {
            try
            {
                var existingWallet = await _context.PrivateWallets.FirstOrDefaultAsync(x => x.Guid == wallet.Guid);

                if (existingWallet == null)
                    throw new ArgumentException("Carteira não encontrada.");

                _context.PrivateWallets.Remove(existingWallet);
                await _context.SaveChangesAsync();
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException("Erro de comunicação com o banco de dados PostgreSQL.\n" + ex.Message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro inesperado \n" + ex.Message);
            }
        }

        public async Task<PrivateWallet> ObterCarteiraPrivadaPorGuid(Wallet wallet)
        {
            try
            {
                var existingWallet = await _context.PrivateWallets
                        .Include(w => w.Transactions)
                        .FirstOrDefaultAsync(x => x.Guid == wallet.Guid);

                if (existingWallet == null)
                    throw new ArgumentException("Carteira não encontrada.");

                return existingWallet;
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException("Erro de comunicação com o banco de dados PostgreSQL.\n" + ex.Message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro inesperado \n" + ex.Message);
            }


        }
    }
}
