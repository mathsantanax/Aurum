using Aurum_Application.DTOs;
using Aurum_Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.Service
{
    internal class UserService : IUserService
    {
        Task IUserService.AddUserAsync(UserDto user)
        {
            throw new NotImplementedException();
        }

        Task IUserService.DeleteUserAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        Task<UserDto> IUserService.GetUserAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        Task IUserService.UpdateUserAsync(UserDto user)
        {
            throw new NotImplementedException();
        }
    }
}
