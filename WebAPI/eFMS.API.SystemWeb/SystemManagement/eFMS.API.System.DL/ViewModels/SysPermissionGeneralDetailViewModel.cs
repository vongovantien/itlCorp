using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.ViewModels
{
    public class SysPermissionGeneralDetailViewModel
    {
        public string ModuleName { get; set; }
        public string ModuleID { get; set; }
        public List<SysPermissionGeneralModel> SysPermissionGenerals { get; set; }
    }
}
