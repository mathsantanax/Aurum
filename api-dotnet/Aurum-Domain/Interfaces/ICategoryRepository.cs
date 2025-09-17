using Aurum_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Domain.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllCategories(User user); // buscar todas as categorias
        Task AddOrUpdatedCategory(Category category); // adcionar ou atualizar categoria
        Task DeleteCategory (Category category); // deletar categoria
    }
}
