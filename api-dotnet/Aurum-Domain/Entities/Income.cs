using Aurum_Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Entities
{
    public class Income : Transaction
    {
        public override TransactionType Type => TransactionType.Income;
        public Income(string description, Money value, Category category, Wallet? wallet = null, SharedWallet? sharedWallet = null)
            : base(description, value, category, wallet, sharedWallet) { }
    }
}
