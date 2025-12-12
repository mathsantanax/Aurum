using Aurum.Applications.DTOs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Applications.DTOs
{
    public record class CategoryDTO
    {
        public string NameCagegory { get; set; } = string.Empty!;
        public string DescriptionCagegory { get; set; } = string.Empty!;
        public TransactionFlowDTO IsIncome { get; set; }
    }
}
