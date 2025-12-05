using Aurum.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Interfaces
{
    public interface ICreditCardRepository
    {
        Task<int> CreateCreditCard(CreditCard creditCard);
        Task<CreditCard> GetCreditCard(Guid id);
    }
}
