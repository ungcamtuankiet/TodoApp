using MRT.Contract.DTOs.Auth;
using MRT.Contract.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> GetIdFromToken();
        //LOGIN
        Task<Authenticator> LoginAsync(LoginDTO loginDTO);
        Task<Authenticator> RefreshToken(string token);
        Task<bool> DeleteRefreshToken(Guid userId);

    }
}
