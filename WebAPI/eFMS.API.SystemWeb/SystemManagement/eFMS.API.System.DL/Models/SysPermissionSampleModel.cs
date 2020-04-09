using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class SysPermissionSampleModel: SysPermissionSample
    {
        public string RoleName { get; set; }
        public Guid OfficeId { get; set; }
        public string UserId { get; set; }
        public Guid PermissionSampleId { get; set; }
        public string NameUserCreated { get; set; }
        public string NameUserModified { get; set; }

        public List<SysPermissionSampleGeneralViewModel> SysPermissionSampleGenerals { get; set; }
        public List<SysPermissionSampleSpecialViewModel> SysPermissionSampleSpecials { get; set; }
    }
}
