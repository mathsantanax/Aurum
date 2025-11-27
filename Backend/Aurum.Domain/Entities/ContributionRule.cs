using Aurum.Domain.Entities.Enums;

namespace Aurum.Domain.Entities
{
    public class ContributionRule
    {
        public ContributionType Type { get; } // Tipo de contribuição
        public decimal? Value { get; } // Valor fixo de contribuição (se aplicável)

        protected ContributionRule() { } // Construtor para EF Core (protegido ou privado)

        public ContributionRule(ContributionType type, decimal? value)
        {
            if (type == ContributionType.Percentage && (value < 0 || value > 100))
            {
                throw new ArgumentException("Porcentagem deve estar entre 0 e 100.");
            }
            if (type != ContributionType.None && value < 0)
            {
                throw new ArgumentException("Valor da contribuição deve ser positivo.");
            }

            Type = type;
            Value = value;
        }
    }
}
