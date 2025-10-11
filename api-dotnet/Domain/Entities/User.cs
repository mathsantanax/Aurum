using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User
    {
        public Guid guid { get; private set; }
        public string? FullName { get; private set; }
        public string? Email { get ; private set; }
        public string? Phone {  get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.Now;
        public bool isActive { get; private set; }

        
    }
}
