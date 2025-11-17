using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AurumDbContext _context;
        public UserRepository(AurumDbContext context)
        {
            this._context = context;
        }
        public async Task<User> GetUser(Guid guid)
        {
            var user = await _context.Users.FindAsync(guid);
            if (user == null)
                throw new Exception("Usuário não encontrado.");
            return user;
        }
    }
}
