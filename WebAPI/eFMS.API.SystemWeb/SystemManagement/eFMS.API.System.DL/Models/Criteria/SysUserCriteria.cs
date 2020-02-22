using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models.Criteria
{
    public class SysUserCriteria
    {
        public string All { get; set; }
        public string Username { get; set; }
        public string EmployeeNameEn { get; set;}
        public string EmployeeNameVn { get; set; }

        public string UserType { get; set; }
        public bool? Active { get; set; }
        //public string 
    }
}
