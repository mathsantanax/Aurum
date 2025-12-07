using Aurum.Domain.Entities.Enums;

namespace Aurum.Domain.Entities
{
    public class SharedWalletMembership
    {
        public Guid Id { get; private set; } // Identificador único da associação

        public Guid UserGuid { get; private set; } // Chave estrangeira para o usuário
        public Guid WalletId { get; private set; } // Chave estrangeira para a carteira compartilhada

        public MemberRole Role { get; private set; } // Papel do membro na carteira compartilhada
        public DateTime JoinedAt { get; private set; } // Data de ingresso na carteira compartilhada
        public ContributionRule? ContribuitionRule { get; private set; } // Tipo de contribuição do membro

        public SharedWalletMembership() { } // Construtor para EF Core (protegido ou privado)

        // Construtor público para criar uma nova associação
        public SharedWalletMembership(Guid userGuid, Guid walletId, MemberRole role, ContributionRule rule)
        {
            Id = Guid.NewGuid();
            UserGuid = userGuid;
            WalletId = walletId;
            Role = role;
            ContribuitionRule = rule ?? throw new ArgumentNullException(nameof(rule));
            JoinedAt = DateTime.UtcNow;
        }

        public void ChangeRole(MemberRole newRole)
        {
            Role = newRole;
        }

        public void UpdateContribuitionRule(ContributionRule newRule)
        {
            ContribuitionRule = newRule ?? throw new ArgumentNullException(nameof(newRule));
        }

    }
}
