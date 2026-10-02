using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Infrastructure.Identity
{
    public class AurumUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty!;
    }
}
