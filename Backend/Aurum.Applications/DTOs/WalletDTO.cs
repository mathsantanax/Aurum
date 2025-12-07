using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Applications.DTOs
{
    public record class WalletDTO
    {
        public string Name { get; set; } = string.Empty!;
    }
}
