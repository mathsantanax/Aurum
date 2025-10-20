using AurumApi.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AurumApi.Models
{
    public abstract class Wallet
    {
        [Key]
        public Guid Guid { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty!;
        [Required]
        public decimal Amount { get; set; } = decimal.Zero;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public WalletType WalletType { get; set; }

        public virtual ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

        public Wallet() { }
    }
}
