using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class SysUserPermissionModel: SysUserPermission
    {
        public string PermissionName { get; set; }
        public string UserTitle { get; set; }
        public string OfficeName { get; set; }
        public List<SysUserPermissionGeneralViewModel> SysUserPermissionGenerals { get; set; }
        public List<SysUserPermissionSpecialViewModel> SysUserPermissionSpecials { get; set; }
    }
    
    public class SysUserPermissionEditModel
    {
        public Guid PermissionSampleId { get; set; }
        public string UserId { get; set; }
        public Guid OfficeId { get; set; }
    }
}
