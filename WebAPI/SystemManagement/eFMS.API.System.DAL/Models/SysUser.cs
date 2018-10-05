using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public short UserGroupId { get; set; }
        public string EmployeeId { get; set; }
        public Guid WorkPlaceId { get; set; }
        public bool? RefuseEmail { get; set; }
        public Guid? LdapObjectGuid { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public SysUserGroup UserGroup { get; set; }
        public CatBranch WorkPlace { get; set; }
    }
}
