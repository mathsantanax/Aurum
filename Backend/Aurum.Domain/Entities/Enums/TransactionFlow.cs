using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities.Enums
{
    public enum TransactionFlow
    {
        Expense, // Despeza (Deduz do saldo)    
        Income, // Receita (Adiciona ao saldo)
        Transfer // Transferência (Entre carteiras)
    }
}
