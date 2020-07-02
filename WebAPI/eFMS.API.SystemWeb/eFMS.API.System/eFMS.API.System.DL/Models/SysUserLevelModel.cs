using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class SysUserLevelModel: SysUserLevel
    {
        public string GroupName { get; set; }
        public string UserName { get; set; }
        public string EmployeeName { get; set; }
        public bool isDup { get; set; }
        public string CompanyName { get; set; }
        public string OfficeName { get; set; }
        public string DepartmentName { get; set; }
        public string GroupAbbrName { get; set; }
        public string CompanyAbbrName { get; set; }
        public string OfficeAbbrName { get; set; }
        public string DepartmentAbbrName { get; set; }
    }
}
