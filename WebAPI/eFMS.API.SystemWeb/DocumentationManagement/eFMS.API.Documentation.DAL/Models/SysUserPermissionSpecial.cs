using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysUserPermissionSpecial
    {
        public Guid Id { get; set; }
        public Guid? UserPermissionId { get; set; }
        public string ModuleId { get; set; }
        public string MenuId { get; set; }
        public string ActionName { get; set; }
        public bool? IsAllow { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
