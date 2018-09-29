using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysRoleMenu
    {
        public int Id { get; set; }
        public short? RoleId { get; set; }
        public string MenuId { get; set; }
        public bool? AllowAccess { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public SysMenu Menu { get; set; }
        public SysRole Role { get; set; }
    }
}
