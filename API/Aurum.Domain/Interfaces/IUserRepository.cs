using Aurum.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Domain.Interfaces
{
    public interface IUserRepository
    {
        public Task<User> GetUser (User user);
        public Task AddUser (User user);
        public Task DeleteUser (User user);
        public Task UpdateUser (User user);

    }
}
