using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Entities
{
    public class Category
    {
        public Guid Guid { get; private set; }
        public string NameCategory { get; private set; } = string.Empty!; // categoria ex: Alimentação, Moradia, etc...
        public string Description { get; private set; } = string.Empty!; // descrição da categoria de gastos ex: NameCategory = Alimentação, Descrição = Mercado, Restaurantes, Bar etc...
        public DateTime CreatedAt { get; private set; }
        public DateTime LastUpdatedAt { get; private set;}

        public Category() { }

        public void AddCategoria(string nameCategory, string descricao)
        {
            this.Guid = Guid.NewGuid();
            this.NameCategory = nameCategory;
            this.Description = descricao;
            this.CreatedAt = DateTime.UtcNow;
        }

        public void UpdateCategoria(string nameCategory, string descricao)
        {
            this.NameCategory = nameCategory;
            this.Description = descricao;
            this.LastUpdatedAt = DateTime.Now;
        }
    }
}
