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
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Guid == wallet.User.Guid);
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

        public Task DeletarCarteira(Wallet wallet)
        {
            throw new NotImplementedException();
        }
    }
}
