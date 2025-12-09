
using Aurum.Domain.Entities.Enums;

namespace Aurum.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; private set; } // Identificador único da categoria
        public string? Name { get; private set; } // Nome da categoria
        public string? Description { get; private set; } // Descrição da categoria
        public TransactionFlow IsIncome { get; private set; } // Indica se a categoria é de receita ou despesa
        public DateTime CreatedAt { get; private set; } // Data de criação
        public Guid UserGuid { get; private set; } // Chave estrangeira para o usuário

        public Category() { } // Construtor para EF Core (protegido ou privado)

        // Construtor público para criar uma nova categoria
        public Category(Guid userGuid, string? name, string? description, TransactionFlow isIncome)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            UserGuid = userGuid;
            CreatedAt = DateTime.UtcNow;
            IsIncome = isIncome;
        }

        // Método para renomear a categoria
        public void Rename(string? newName)
        {
            if(string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("O nome da categoria não pode ser vazio.", nameof(newName));
            Name = newName;
        }
    }
}
