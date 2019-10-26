using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.ViewModels
{
    public class SysPermissionSampleViewModel: SysPermissionSample
    {
        public List<SysPermissionSampleGeneralViewModel> SysPermissionSampleGenerals { get; set; }
        public List<SysPermissionSampleSpecialViewModel> SysPermissionSampleSpecials { get; set; }
    }
}
