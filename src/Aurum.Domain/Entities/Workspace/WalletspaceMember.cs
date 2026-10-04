using Aurum.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Domain.Entities.Workspace
{
    public class WalletspaceMember : BaseEntity
    {
        public Guid WalletspaceId { get; private set; }

        public Guid UserId { get; private set; }

        public WalletspaceRole Role { get; private set; }

        public DateTime JoinedAt { get; private set; }

        public Walletspace Walletspace { get; private set; } = null!;

        private WalletspaceMember()
        {
        }

        public WalletspaceMember(
                Guid walletspaceId,
                Guid userId,
                WalletspaceRole role)
        {
            if (walletspaceId == Guid.Empty)
                throw new ArgumentException(
                    "A Walletspace é obrigatória.",
                    nameof(walletspaceId));

            if (userId == Guid.Empty)
                throw new ArgumentException(
                    "O usuário é obrigatório.",
                    nameof(userId));
            if (!Enum.IsDefined(role))
                throw new ArgumentOutOfRangeException(nameof(role));

            WalletspaceId = walletspaceId;
            UserId = userId;
            Role = role;
            JoinedAt = DateTime.UtcNow;

            SetCreatedInfo(userId);
        }

        internal void ChangeRole(
            WalletspaceRole role,
            Guid updatedBy)
        {
            if (!Enum.IsDefined(role))
                throw new ArgumentOutOfRangeException(nameof(role));

            Role = role;
            MarkAsUpdated(updatedBy);
        }
    }
}
