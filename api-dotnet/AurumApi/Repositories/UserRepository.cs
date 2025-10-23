using AurumApi.Interfaces;
using AurumApi.Models;
using AurumApi.Persistence;
using Microsoft.EntityFrameworkCore;

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

        public async Task DeleteUser(User user)
        {
            var verifyUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Guid == user.Guid);
            if (verifyUser == null)
                throw new Exception("Usuário Inválido.");

            try
            {
                dbContext.Users.Remove(verifyUser);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            { 
                throw new Exception(ex.Message); 
            }
        }

        public async Task<User> GetUser(User user)
        {
            try
            {
                var verifyUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Guid == user.Guid);
                if (verifyUser.Equals(null))
                    throw new Exception("Usuário Inválido.");

                return verifyUser;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateUser(User user)
        {
            try
            {
                var verifyUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Guid == user.Guid);
                if (verifyUser.Equals(null))
                    throw new Exception("Usuário Inválido.");

                verifyUser = new User(user.Guid, user.Name, user.Email, user.PhoneNumber);
                dbContext.Users.Update(verifyUser);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
