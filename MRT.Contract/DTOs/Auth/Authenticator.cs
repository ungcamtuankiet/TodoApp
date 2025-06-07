using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Contract.DTOs.Auth
{
    public class Authenticator
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}
