using System;
using System.Collections.Generic;

namespace eFMS.IdentityServer.Service.Models
{
    public partial class SysRole
    {
        public SysRole()
        {
            SysGroupRole = new HashSet<SysGroupRole>();
            SysRoleMenu = new HashSet<SysRoleMenu>();
            SysRolePermission = new HashSet<SysRolePermission>();
        }

        public short Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual ICollection<SysGroupRole> SysGroupRole { get; set; }
        public virtual ICollection<SysRoleMenu> SysRoleMenu { get; set; }
        public virtual ICollection<SysRolePermission> SysRolePermission { get; set; }
    }
}
