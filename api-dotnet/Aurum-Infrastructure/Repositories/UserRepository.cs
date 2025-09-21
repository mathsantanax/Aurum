using Aurum_Domain.Entities;
using Aurum_Domain.Interfaces;
using Aurum_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Infrastructure.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private readonly InfraContext _infraContext;

        public UserRepository(InfraContext infraContext)
        {
            _infraContext = infraContext;
        }

        public async Task AddAsync(User user)
        {
            await _infraContext.Users.AddAsync(user);
            await _infraContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            var existingUser = await _infraContext.Users.FindAsync(user.Id);
            if (existingUser != null)
            {
                _infraContext.Users.Remove(existingUser);
                await _infraContext.SaveChangesAsync();
            }
        }

        public async Task<User> GetUser(Guid guid)
        {
            return await _infraContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == guid);
        }

        public async Task UpdateAsync(User user)
        {
            _infraContext.Users.Update(user);
            await _infraContext.SaveChangesAsync();
        }
    }
}
