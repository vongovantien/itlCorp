using System;
using System.Collections.Generic;

namespace eFMS.API.Operation.Service.Models
{
    public partial class SysRolePermission
    {
        public int Id { get; set; }
        public short RoleId { get; set; }
        public string MenuId { get; set; }
        public short PermissionId { get; set; }
        public short? OtherIntructionId { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual SysMenu Menu { get; set; }
        public virtual SysMenuPermissionInstruction OtherIntruction { get; set; }
        public virtual SysPermission Permission { get; set; }
        public virtual SysRole Role { get; set; }
    }
}
