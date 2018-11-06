using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class SysRole
    {
        public SysRole()
        {
            SysRoleMenu = new HashSet<SysRoleMenu>();
            SysRolePermission = new HashSet<SysRolePermission>();
            SysUserRole = new HashSet<SysUserRole>();
        }

        public short Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public ICollection<SysRoleMenu> SysRoleMenu { get; set; }
        public ICollection<SysRolePermission> SysRolePermission { get; set; }
        public ICollection<SysUserRole> SysUserRole { get; set; }
    }
}
