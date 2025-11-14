using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string FullName { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }

    }
}
