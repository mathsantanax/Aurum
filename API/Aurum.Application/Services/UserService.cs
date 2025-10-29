using Aurum.Application.DTOs;
using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        { this._userRepository = userRepository; }

        public async Task AddUserAsync(UserDTO userDTO)
        {
            if (userDTO == null)
                throw new ArgumentNullException(nameof(userDTO));

            var user = new User(userDTO.Name, userDTO.Email, userDTO.PhoneNumber);

            await _userRepository.AddUser(user);
        }

        public async Task UpdateUserAsync(UserDTO userDTO)
        {
            if (userDTO == null)
                throw new ArgumentNullException(nameof(userDTO), "O DTO do usuário não pode ser nulo.");

            var user = new User();

            user.UpdateUser(userDTO.Guid, userDTO.Name, userDTO.Email, userDTO.PhoneNumber);

            await _userRepository.UpdateUser(user);
        }
    }
}
