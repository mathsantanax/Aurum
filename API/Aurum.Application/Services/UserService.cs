using Aurum.Application.DTOs;
using Aurum.Domain.Entities;
using Aurum.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Application.Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private readonly IUserRepository _userRepository;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, IUserRepository userRepository)
        { 
            this._userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task RegisterUser(UserDTO request)
        {
            try
            {
                var user = new User();
                user.RegisterUser(request.Name, request.Email, request.PhoneNumber, request.PassWord, request.ConfirmedPassword);
                
                var result = await _userManager.CreateAsync(user, user.PasswordHash!);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            catch(Exception ex)    
            {
                throw new Exception(ex.Message);    
            }
        }

        public async Task<User> LoginUser(UserDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PassWord))
                throw new ApplicationException("Email e senha são obrigatórios.");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new ApplicationException("Email ou senha inválidos.");

            // Verifica se a senha é válida
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.PassWord);
            if (!isPasswordValid)
                throw new ApplicationException("Email ou senha inválidos.");

            // Opcional: Atualiza o SecurityStamp para expirar tokens antigos
            await _userManager.UpdateSecurityStampAsync(user);

            return user;
        }


        public async Task<User> GetUserAsync(UserDTO userDTO)
        {
            var user = new User();
            user.GetUser(userDTO.Guid, userDTO.PhoneNumber);
            
           return await _userRepository.GetUser(user);
        }

        public async Task AddUserAsync(UserDTO userDTO)
        {
            if (userDTO == null)
                throw new ArgumentNullException(nameof(userDTO));

            var user = new User();
            user.AddUser(userDTO.Name, userDTO.Email, userDTO.PhoneNumber);

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

        public async Task DeleteUserAsync(UserDTO userDTO)
        {
            if (userDTO == null)
                throw new ArgumentNullException(nameof(userDTO), "O DTO do usuário não pode ser nulo.");
            var user = new User();

            user.UpdateUser(userDTO.Guid, userDTO.Name, userDTO.Email, userDTO.PhoneNumber);

            await _userRepository.DeleteUser(user);
        }
    }
}
