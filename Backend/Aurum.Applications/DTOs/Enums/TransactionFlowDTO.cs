using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Applications.DTOs.Enums
{
    public enum TransactionFlowDTO
    {
        Expense, // Despeza (Deduz do saldo)    
        Income, // Receita (Adiciona ao saldo)
        Transfer // Transferência (Entre carteiras)
    }
}
