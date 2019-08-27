using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class SysUserGroup
    {
        public int Id { get; set; }
        public short GroupId { get; set; }
        public string UserId { get; set; }
        public int? DepartmentId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual CatDepartment Department { get; set; }
        public virtual SysGroup Group { get; set; }
        public virtual SysUser User { get; set; }
    }
}
