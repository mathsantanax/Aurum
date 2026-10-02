using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Application.Interfaces.Auth
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        bool IsAuthenticated { get; }
    }
}
