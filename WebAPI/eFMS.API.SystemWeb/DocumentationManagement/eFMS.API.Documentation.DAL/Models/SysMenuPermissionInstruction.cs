using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysMenuPermissionInstruction
    {
        public SysMenuPermissionInstruction()
        {
            SysRolePermission = new HashSet<SysRolePermission>();
        }

        public short Id { get; set; }
        public string MenuId { get; set; }
        public short PermissionId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual ICollection<SysRolePermission> SysRolePermission { get; set; }
    }
}
