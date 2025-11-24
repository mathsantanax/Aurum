using Aurum.Domain.Entities.Enums;

namespace Aurum.Domain.Entities
{
    public class Invite
    {
        public Guid Id { get; private set; } // Identificador único do convite
        public Guid WalletId { get; private set; } // Identificador da carteira associada
        public Guid SendUserId { get; private set; } // Identificador do usuário que enviou o convite
        public Guid InvitedUserId { get; private set; } // Identificador do usuário convidado
        public DateTime SendAt { get; private set; } // Data de envio do convite
        public DateTime ExpireAt { get; private set; }// Data de expiração do convite (7 dias após o envio)
        public InviteStatus Status { get; private set; } // Status do convite

        protected Invite() { } // Construtor para EF Core (protegido ou privado)

        // Construtor público para criar um novo convite
        public Invite(Guid walletId, Guid sendUserId, Guid invitedUserId)
        {
            Id = Guid.NewGuid();
            WalletId = walletId;
            SendUserId = sendUserId;
            InvitedUserId = invitedUserId;
            SendAt = DateTime.UtcNow;
            ExpireAt = SendAt.AddDays(7);
            Status = InviteStatus.Pending;
        }

        // Método para aceitar o convite
        public void Accept()
        {
            if (Status != InviteStatus.Pending)
                throw new InvalidOperationException("Somente convites pendentes podem ser aceitos.");
            Status = InviteStatus.Accepted;
        }

        // Método para recusar o convite
        public void Decline()
        {
            if (Status != InviteStatus.Pending)
                throw new InvalidOperationException("Somente convites pendentes podem ser recusados.");
            Status = InviteStatus.Declined;
        }

    }
}
