using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Contract.DTOs.User
{
    public class ResetPasswordDTO
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
