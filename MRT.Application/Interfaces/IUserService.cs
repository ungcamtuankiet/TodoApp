using MRT.Contract.DTOs.User;
using MRT.Contracts.Abstractions.Shared;
using MRT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Application.Interfaces
{
    public interface IUserService
    {
        public Task<Result<object>> CreateNewUser(CreateUserDTO user);
        Task<IList<ApplicationUser>> GetALl();
        Task<ApplicationUser> GetByEmail(string email);
        Task UpdateUserAsync(ApplicationUser user);


        Task<UserDTO> GetUserById(Guid id);
        Task<Result<UserDTO>> GetCurrentUserById();
    }
}
