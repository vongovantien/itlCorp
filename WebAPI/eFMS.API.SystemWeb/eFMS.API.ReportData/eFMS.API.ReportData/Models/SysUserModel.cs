using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class SysUserModel
    {
        public string UserName { get; set; }
        public string EmployeeNameEN { get; set; }
        public string EmployeeNameVN { get; set; }
        public string Title { get; set; }
        public string UserType { get; set; }
        public string Role { get; set; }
        public string LevelPermission { get; set; }
        public string Company { get; set; }
        public string Office { get; set; }
        public string Department { get; set; }
        public string Group { get; set; }
        public string WorkingStatus { get; set; }
        public bool? Active { get; set; }
        public string UserRole { get; set; }
    }
}
