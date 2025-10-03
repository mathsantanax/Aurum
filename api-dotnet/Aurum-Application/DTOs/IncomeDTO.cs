using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.DTOs
{
    public record class IncomeDTO : TransactionDTO
    {
        public string? Source { get; set; }
    }
}
