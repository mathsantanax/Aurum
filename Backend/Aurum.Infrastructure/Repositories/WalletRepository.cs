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
        public async Task<Wallet> CreateWallet(Wallet wallet)
        {
            try
            {
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear(); // Limpa o rastreamento para liberar memória
                return wallet;

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

        public async Task<Wallet> GetWalletByGuid(Guid walletGuid)
        {
            var result = await _context.Wallets
                            .Where(w => w.Id.Equals(walletGuid))
                            .Include(w => w.SharedWalletMemberships)
                            .AsNoTracking()
                            .AsSplitQuery()
                            .FirstOrDefaultAsync();
            return result ?? throw new AppException($"Não Existe nenhuma cateira com id {walletGuid}");
        }

        // Obtém todas as carteiras associadas a um usuário específico.
        public async Task<IReadOnlyList<Wallet>> GetAllWallets(Guid userId)
        {
            try
            {
                // Consulta para obter todas as carteiras onde o usuário é membro.
                var result = await _context.Wallets
                                 .Where(w => w.SharedWalletMemberships.Any(m => m.UserGuid == userId)) // Filtra carteiras onde o usuário é membro
                                 .Include(w => w.Transactions) // Inclui as transações associadas
                                 .Include(w => w.CreditCards) // Inclui os cartões de crédito associados
                                 .Include(w => w.SharedWalletMemberships) // Inclui os membros da carteira compartilhada
                                 .AsNoTracking() // Evita o rastreamento para melhorar o desempenho em consultas somente leitura
                                 .AsSplitQuery() // habilitando consultas divididas
                                 .ToListAsync();
                return result;

            }
            catch(DbException ex)
            {
                throw new AppException($"Erro ao acessar o banco de dados. {ex.Message}", ex.HResult);
            }
        }

        public async Task<Wallet> GetWalletById(Guid WalletId, Guid UserId)
        {
            try
            {
                var result = await _context.Wallets
                            .Where(w => w.Id.Equals(WalletId) && w.SharedWalletMemberships.Any(m => m.UserGuid == UserId))
                            .Include(w => w.Transactions)
                            .Include(w => w.CreditCards)
                            .Include(w => w.SharedWalletMemberships)
                            .AsSplitQuery()
                            .AsNoTracking()
                            .FirstOrDefaultAsync();
                return result ?? throw new AppException($"Não Existe nenhuma cateira com id {WalletId}");
            }
            catch (DbException ex)
            {
                throw new AppException($"Erro ao acessar o banco de dados. {ex.Message}", ex.HResult);
            }
        }

        public async Task<Wallet> UpdateWallet(Wallet wallet)
        {
            try
            {
                _context.Wallets.Update(wallet);
                //_context.Wallets.Attach(wallet);
                //_context.Entry(wallet).Property(w => w.Name).IsModified = true;
                //_context.Entry(wallet).Property(w => w.UpdatedAt).IsModified = true;
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear(); // Limpa o rastreamento para liberar memória
                return wallet;
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
    }
}
