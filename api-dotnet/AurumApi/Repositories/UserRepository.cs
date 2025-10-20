using AurumApi.Interfaces;
using AurumApi.Models;
using AurumApi.Persistence;

namespace AurumApi.Repositories
{
    public class UserRepository : IUserInterface
    {
        private readonly AurumDbContext dbContext;

        public UserRepository(AurumDbContext context)
        {
            this.dbContext = context;
        }
        public async Task AddUser(User user)
        {
            try
            {
                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task DeleteUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUser(User user)
        {
            throw new NotImplementedException();
        }
    }
}
