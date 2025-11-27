using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities.Enums
{
    public enum InviteStatus
    {
        Pending, // Convite pendente
        Accepted, // Convite aceito
        Declined, // Convite recusado
        Expired // Convite expirado
    }
}
