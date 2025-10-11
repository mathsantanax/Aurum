using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.DTOs
{
    public record class WalletDto
    {
        public Guid guid {  get; set; }
        public string? name { get; set; } // nome da carteira
        public decimal? Balance { get; set; }
        public Guid UserId { get; set; }
    }
}
