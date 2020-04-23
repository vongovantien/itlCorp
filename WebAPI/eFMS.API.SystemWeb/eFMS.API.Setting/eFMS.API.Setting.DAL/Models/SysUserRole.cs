using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class SysUserRole
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public short GroupId { get; set; }
        public short RoleId { get; set; }
        public byte Buid { get; set; }
        public string BranchId { get; set; }
        public short DepartmentId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
