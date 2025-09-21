using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.DTOs
{
    public record class TransactionDTO
    {
        public Guid guid { get; set; }
        public string? Description { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public Guid CategoryId { get; set; }
        public Guid WalletId { get; set; }
    }
}
