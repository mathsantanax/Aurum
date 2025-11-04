using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Application.DTOs
{
    public record class PrivateWalletDTO
    {
        public Guid guid { get; set; }
        [Required]
        public UserDTO user {  get; set; } = null!;
        [Required]
        public string NameWallet { get; set; } = string.Empty!;
    }
}
