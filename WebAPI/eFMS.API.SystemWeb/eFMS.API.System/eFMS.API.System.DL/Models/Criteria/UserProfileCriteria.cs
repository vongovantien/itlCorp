using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models.Criteria
{
    public class UserProfileCriteria
    {
        public string Avatar { get; set; }
        public string EmployeeNameVn { get; set; }
        public string EmployeeNameEn { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string Tel { get; set; }
        public string Description { get; set; }
    }
}
