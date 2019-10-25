using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysPermissionSampleSpecial
    {
        public short Id { get; set; }
        public short PermissionId { get; set; }
        public string ModuleId { get; set; }
        public string MenuId { get; set; }
        public string ActionName { get; set; }
        public bool? IsAllow { get; set; }
    }
}
