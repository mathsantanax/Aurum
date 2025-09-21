using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Entities
{
    public class Category
    {
        public Guid Id { get; private set; }
        public string? Description { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdateAt { get; private set; }
        public Guid UserId { get; private set; }
        public User? User { get; private set; }

        private Category() { }

        public Category(string description)
        {
            Id = Guid.NewGuid();
            SetDescription(description);
            CreatedAt = DateTime.UtcNow;
        }

        public void Rename(string newDescription)
        {
            SetDescription(newDescription);
            UpdateAt = DateTime.UtcNow;
        }

        private void SetDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Descrição é obrigatória.");
            Description = description;
        }
    }
}
