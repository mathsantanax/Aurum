using Aurum.Applications.DTOs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Applications.DTOs
{
    public class TransactionDTO
    {
        public Guid WalletGuid  { get; set; }
        public Guid CategoryGuid  { get; set; }
        public decimal Amount  { get; set; }
        public string? Description  { get; set; }
        public TransactionFlowDTO type { get; set; }

        //public Guid? CreditCardGuid  { get; set; }
        //public int? InstallmentNumber  { get; set; }
    }
}
