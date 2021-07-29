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
        public string CompanyId { get; set; }
        //public string 

        public string Title { get; set; }
        public string Company { get; set; }
        public string Office { get; set; }
    }
}
