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
    }
}
