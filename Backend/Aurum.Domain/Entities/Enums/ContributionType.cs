using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities.Enums
{
    public enum ContributionType
    {
        Percentage, // Contribuição baseada em porcentagem
        FixedAmount, // Contribuição baseada em valor fixo
        None // Sem contribuição
    }
}
