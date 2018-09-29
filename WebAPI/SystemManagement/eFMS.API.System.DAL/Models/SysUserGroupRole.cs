using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysUserGroupRole
    {
        public int Id { get; set; }
        public short UserGroupId { get; set; }
        public short RoleId { get; set; }
        public bool? AllHub { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public bool? AllBranch { get; set; }

        public SysRole Role { get; set; }
        public SysUserGroup UserGroup { get; set; }
    }
}
