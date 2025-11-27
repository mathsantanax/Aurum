using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Application.DTOs
{
    public record class CategoryDTO
    {
        public string Title { get; set; } = string.Empty!;
        public string Description {  get; set; } = string.Empty!;    
    }

    public record GetCategoryDTO
    {
        public Guid guid { get; set; }
    }
}
