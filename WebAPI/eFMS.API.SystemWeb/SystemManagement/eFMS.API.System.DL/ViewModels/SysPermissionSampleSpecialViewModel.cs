using eFMS.API.System.DL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.ViewModels
{
    public class SysPermissionSampleSpecialViewModel
    {
        public short PermissionID { get; set; }
        public string ModuleName { get; set; }
        public string ModuleID { get; set; }
        public List<SysPermissionSpecialViewModel> SysPermissionSpecials { get; set; }
    }
}
