using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class LoginModel
    {
        public string userName { get; set; }
        public string email { get; set; }
        public string token { get; set; }
        public bool success { get; set; }
        public string error { get; set; }
        

    }
}
