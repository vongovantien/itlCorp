using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysGroup
    {
        public SysGroup()
        {
            SysGroupRole = new HashSet<SysGroupRole>();
            SysUserGroup = new HashSet<SysUserGroup>();
        }

        public short Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? DepartmentId { get; set; }
        public short ParentId { get; set; }
        public string ManagerId { get; set; }
        public string Decription { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual CatDepartment Department { get; set; }
        public virtual ICollection<SysGroupRole> SysGroupRole { get; set; }
        public virtual ICollection<SysUserGroup> SysUserGroup { get; set; }
    }
}
