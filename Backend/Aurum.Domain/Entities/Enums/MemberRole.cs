using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities.Enums
{
    public enum MemberRole
    {
        Owner, // Criador da carteira compartilhada
        Admin, // Pode gerenciar membros e transações
        Member, // Pode adicionar transações
        Viewer // Pode apenas visualizar a carteira
    }
}
