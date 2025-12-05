using Aurum.Applications.Exceptions;
using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Infrastructure.Repositories
{
    public class CreditCardRepository(AppDbContext appContext) : ICreditCardRepository
    {
        private readonly AppDbContext _context = appContext;

        public async Task<int> CreateCreditCard(CreditCard creditCard)
        {
            try
            {
                _context.CreditCards.Add(creditCard);
                return await _context.SaveChangesAsync();
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

        public async Task<CreditCard> GetCreditCard(Guid id)
        {
            try
            {
                var creditCard = await _context.CreditCards
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cc => cc.Id == id);
                return creditCard ?? throw new AppException($"Não Existe nenhum cartão de crédito com id {id}");
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
