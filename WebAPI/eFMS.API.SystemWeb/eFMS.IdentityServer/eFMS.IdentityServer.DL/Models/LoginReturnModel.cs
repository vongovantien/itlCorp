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
        public int? departmentId { get; set; }
        public int? groupId { get; set; }
        public string email { get; set; }
        public string token { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public string NameEn { get; set; }
        public string NameVn { get; set; }
        public string BankName { get; set; }
        public string BankAccountNo { get; set; }
        public string Photo { get; set; }
        public string Title { get; set; }
    }
}
