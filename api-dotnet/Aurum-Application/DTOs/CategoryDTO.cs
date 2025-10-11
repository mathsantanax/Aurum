using Aurum_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Aurum_Application.DTOs
{
    public record class CategoryDTO
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public Guid UserId { get; set; }

        public static CategoryDTO FromEntity(Category category)
        {
            return new CategoryDTO
            {
                Id = category.Id,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdateAt = category.UpdateAt,
                UserId = category.UserId,
            };
        }
        public Category ToEntity()
        {
            return new Category(Description!);
        }
    }
}
