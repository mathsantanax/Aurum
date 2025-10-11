using Aurum_Application.DTOs;
using Aurum_Application.Exceptions;
using Aurum_Application.Interfaces;
using Aurum_Domain.Entities;
using Aurum_Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository) => this.userRepository = userRepository;

        public async Task AddUserAsync(UserDto user)
        {
            if (user == null)
                throw new AppException("Usuário não pode ser nulo.", 400);
            var entity = new User(user.Name, user.Email, user.Phone);
            await userRepository.AddAsync(entity);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var entity = await userRepository.GetUser(id);
            if (entity == null)
                throw new AppException("Usuário não encontrado.", 404);

            await userRepository.DeleteAsync(entity);
        }

        public async Task<UserDto> GetUserAsync(Guid id)
        {
            var entity = await userRepository.GetUser(id);
            if (entity == null)
                throw new AppException("Usuário não encontrado.", 404);

            var userdto = new UserDto
            {
                Id = id,
                Name = entity.FullName,
                Email = entity.Email,
                Phone = entity.Phone,
            };

            return userdto;
        }

        public async Task UpdateUserAsync(UserDto user)
        {
            var entity = new User(user.Id, user.Name, user.Email, user.Phone);
            if (entity == null)
                throw new AppException("Usuário não pode ser nulo.", 400);
            await userRepository.DeleteAsync(entity);
        }
    }
}
