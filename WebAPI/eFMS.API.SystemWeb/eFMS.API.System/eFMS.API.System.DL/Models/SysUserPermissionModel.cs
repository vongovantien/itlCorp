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
        public string OfficeAbbrName { get; set; }
        public string Name { get; set; }
        public Guid Buid { get; set; }
        public string UserName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAbbrName { get; set; }
        public string NameUserCreated { get; set; }
        public string NameUserModified { get; set; }
        public List<SysUserPermissionGeneralViewModel> SysPermissionSampleGenerals { get; set; }
        public List<SysUserPermissionSpecialViewModel> SysPermissionSampleSpecials { get; set; }
    }
    
    public class SysUserPermissionEditModel
    {
        public Guid? Id { get; set; }
        public Guid PermissionSampleId { get; set; }
        public string UserId { get; set; }
        public Guid OfficeId { get; set; }
    }
}
