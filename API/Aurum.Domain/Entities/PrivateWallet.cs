using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Aurum.Domain.Entities.Enums;

namespace Aurum.Domain.Entities
{
    public class PrivateWallet : Wallet
    {
        [ForeignKey("OwnerGuid")]
        public Guid OwnerGuid { get; private set; }
        public virtual User User { get; private set; } = null!;

        public PrivateWallet() { }
        public void CriarCarteira(string name, User ownerUser)
        {
            this.Guid = Guid.NewGuid();
            this.OwnerGuid = ownerUser.Id;
            this.User = ownerUser;
            this.Amount = 0;
            this.Name = name;
            this.WalletType = WalletType.Private;
            this.CreatedAt = DateTime.UtcNow;
        }

        public void ObterCarteiraPrivada(Guid guid, string name, User ownerUser)
        {
            this.Guid = guid;
            this.OwnerGuid = ownerUser.Id;
            this.User = ownerUser;
            this.Name = name;
        }
    }
}
