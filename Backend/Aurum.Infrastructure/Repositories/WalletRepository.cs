using Aurum.Applications.Exceptions;
using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Aurum.Infrastructure.Repositories
{
    public class WalletRepository(AppDbContext context) : IWalletRepository
    {
        private readonly AppDbContext _context = context;

        // Cria uma nova carteira no banco de dados.
        public Task<Wallet> CreateWallet(Wallet wallet)
        {
            try
            {
                _context.Wallets.Add(wallet);
                _context.SaveChanges();
                return Task.FromResult(wallet);

            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("violates unique constraint") == true)
                {
                    throw new ValidationException("O nome do item já existe. Escolha outro nome.");
                }

                // Se for outro erro de banco, relança como um erro 500 de aplicação.
                throw new AppException($"Erro ao salvar o item no banco de dados. {ex.Message}", ex.HResult);
            }
        }

        public Task<bool> DeleteWallet(Guid WalletId, Guid UserId)
        {
            throw new NotImplementedException();
        }

        // Obtém todas as carteiras associadas a um usuário específico.
        public async Task<IReadOnlyList<Wallet>> GetAllWallets(Guid UserId)
        {
            try
            {
                // Obtém os IDs das carteiras das quais o usuário é membro.
                var wallets = await _context.Set<SharedWalletMembership>()
                                        .Where(m => m.UserGuid == UserId)
                                        .Select(m => m.WalletId) // Seleciona apenas os IDs das carteiras
                                        .Distinct()
                                        .ToListAsync();

    

                // Obtém as carteiras completas com base nos IDs obtidos.
                var result = await _context.Wallets
                                            .Where(w => wallets.Contains(w.Id))
                                            .Include(w => w.Transactions) // Inclui as transações associadas
                                            .Include(w => w.CreditCards) // Inclui os cartões de crédito associados
                                            .Include(w => w.SharedWalletMemberships) // Inclui os membros compartilhados
                                            .ToListAsync();

                return result;
            }
            catch(DbException ex)
            {
                throw new AppException($"Erro ao acessar o banco de dados. {ex.Message}", ex.HResult);
            }
        }

        public Task<Wallet> GetWalletById(Guid WalletId, Guid UserId)
        {
            throw new NotImplementedException();
        }

        public Task<Wallet> UpdateWallet(Wallet wallet, Guid UserId)
        {
            throw new NotImplementedException();
        }
    }
}
