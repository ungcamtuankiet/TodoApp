using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Contract.DTOs.User
{
    public class ForgotPasswordRequestDTO
    {
        public string EmailOrPhoneNumber { get; set; }
    }
}
