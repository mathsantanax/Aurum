using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }
        public Guid CreatedBy { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        protected void SetCreatedInfo(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException(
                    "O usuário criador é obrigatório.",
                    nameof(userId));

            CreatedBy = userId;
        }

        protected void MarkAsUpdated(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException(
                    "O usuário responsável pela alteração é obrigatório.",
                    nameof(userId));

            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = userId;
        }

    }
}
