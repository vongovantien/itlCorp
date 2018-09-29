using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysPermission
    {
        public SysPermission()
        {
            SysRolePermission = new HashSet<SysRolePermission>();
        }

        public short Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? RequireAccessingForm { get; set; }

        public ICollection<SysRolePermission> SysRolePermission { get; set; }
    }
}
