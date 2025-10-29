using AurumApi.Models;

namespace AurumApi.Interfaces
{
    public interface IUserInterface
    {
        Task<User> GetUser(User user);
        Task<User> GetFromNumber(User number);
        Task AddUser (User user);
        Task DeleteUser (User user);
        Task UpdateUser (User user);
    }
}
