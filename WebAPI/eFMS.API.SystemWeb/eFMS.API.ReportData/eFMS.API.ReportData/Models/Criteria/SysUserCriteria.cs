using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class SysUserCriteria
    {
        public string All { get; set; }
        public string Username { get; set; }
        public string EmployeeNameEn { get; set; }
        public string UserType { get; set; }
        public bool? Active { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public string Office { get; set; }
    }
}
