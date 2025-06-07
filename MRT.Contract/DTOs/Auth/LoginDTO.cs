using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Contract.DTOs.Auth
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
