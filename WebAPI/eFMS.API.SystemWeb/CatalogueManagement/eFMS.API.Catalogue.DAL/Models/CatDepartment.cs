using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatDepartment
    {
        public CatDepartment()
        {
            SysUserGroup = new HashSet<SysUserGroup>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string DeptName { get; set; }
        public string Description { get; set; }
        public Guid? BranchId { get; set; }
        public string ManagerId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual SysBranch Branch { get; set; }
        public virtual ICollection<SysUserGroup> SysUserGroup { get; set; }
    }
}
