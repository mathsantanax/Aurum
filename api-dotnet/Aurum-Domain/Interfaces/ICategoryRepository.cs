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
        Task<List<Category>> GetAllCategories(User user);   // Buscar todas as categorias de um usuário
        Task<Category?> GetById(Guid id);                   // Buscar categoria por ID
        Task<Category?> GetByName(User user, string name);  // Buscar categoria pelo nome (opcional)
        Task AddOrUpdateCategory(Category category);        // Adicionar ou atualizar categoria
        Task DeleteCategory(Category category);             // Deletar categoria
    }
}
