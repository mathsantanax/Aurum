using Aurum_Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Entities
{
    public class Cost : Transaction
    {
        public Cost(string description, Money value, Category category)
             : base(description, value, category) { }

    }
}
