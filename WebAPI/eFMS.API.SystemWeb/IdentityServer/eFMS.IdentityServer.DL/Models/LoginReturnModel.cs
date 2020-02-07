using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class LoginReturnModel
    {
        public string userName { get; set; }
        public string idUser { get; set; }
        public Guid? companyId { get; set; }
        public Guid? officeId { get; set; }
        public string email { get; set; }
        public string token { get; set; }
        public bool status { get; set; }
        public string message { get; set; }        

    }
}
