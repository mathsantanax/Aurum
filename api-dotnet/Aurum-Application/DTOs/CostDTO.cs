using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.DTOs
{
    public record class CostDTO : TransactionDTO
    {
        public string? ExpenseType { get; set; }
    }
}
