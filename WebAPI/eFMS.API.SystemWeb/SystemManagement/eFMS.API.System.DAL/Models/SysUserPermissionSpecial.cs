using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysUserPermissionSpecial
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid? OfficeId { get; set; }
        public string ModuleId { get; set; }
        public string MenuId { get; set; }
        public string ActionName { get; set; }
        public bool? IsAllow { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
