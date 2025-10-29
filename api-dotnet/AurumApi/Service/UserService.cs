using AurumApi.Dtos;
using AurumApi.Interfaces;

namespace AurumApi.Service
{
    public class UserService
    {
        private readonly IUserInterface _userInterface;

        public UserService(IUserInterface userInterface)
        {
            _userInterface = userInterface;
        }


        public async Task<UserDTO> GetUser(UserDTO request)
        {
            if(request.guid != Guid.Empty)
            {
                var user = await _userInterface.GetUser(new Models.User(request.nome));
                if (user == null)
                    return null;

                return new UserDTO(user.Guid,
                                    user.Name,
                                    user.Email,
                                    user.PhoneNumber);

            }
            if (request.phone != null)
            {
                var user = await _userInterface.GetFromNumber(new Models.User(request.phone));
                if (user == null)
                    return null;

                return new UserDTO(user.Guid, user.Name, user.Email, user.PhoneNumber);
            }

            return null;

        }

        public async Task AddUser(UserDTO request)
        {
            await _userInterface.AddUser(new Models.User(request.nome, request.email, request.phone, "null"));
        }

        public async Task UpdateUser(UserDTO request)
        {
            await _userInterface.UpdateUser(new Models.User(request.guid, request.nome, request.email, request.phone));
        }

        public async Task DeleteUser(UserDTO request)
        {
            await _userInterface.DeleteUser(new Models.User(request.guid));
        }
    }
}
