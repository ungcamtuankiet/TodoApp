using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Contract.DTOs.User
{
    public class CreateUserDTO
    {
        public string UserName { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
    }
}
