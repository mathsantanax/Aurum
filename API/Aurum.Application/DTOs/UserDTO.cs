using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Application.DTOs
{
    public record class UserDTO
    {

        [Key]
        public Guid Guid { get; set; }
        [StringLength(250)]
        public string Name { get; set; } = string.Empty!;
        [StringLength(100)]
        public string Email { get; set; } = string.Empty!;
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty!;
        public string PassWord { get; set; } = string.Empty!;
        public string ConfirmedPassword { get; set; } = string.Empty!;
    }
}
