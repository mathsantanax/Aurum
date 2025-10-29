using Aurum.Domain.Entities.Enums;

namespace Aurum.Domain.Entities
{
    public abstract class Wallet
    {
        public Guid Guid { get; set; }
        public string Name { get; set; } = string.Empty!;

        public decimal Amount { get; set; } = decimal.Zero;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public WalletType WalletType { get; set; }

        public virtual ICollection<Transactions> Transactions { get; private set; } = new List<Transactions>();

        public Wallet() { }
    }
}
