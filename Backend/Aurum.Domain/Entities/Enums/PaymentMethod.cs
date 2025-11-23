using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities.Enums
{
    public enum PaymentMethod
    {
        Debit,      // Cartão de Débito ou Dinheiro (Débito Imediato)
        CreditCard, // Cartão de Crédito (Débito Atrasado/Passivo)
        Pix,        // Pix (Débito Imediato)
        Transfer,   // Transferência
        BillPayment // Tipo especial para o registro do Pagamento da Fatura do Cartão
    }
}
