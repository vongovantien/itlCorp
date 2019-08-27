using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysUser
    {
        public SysUser()
        {
            SysAuthorizationAssignToNavigation = new HashSet<SysAuthorization>();
            SysAuthorizationUser = new HashSet<SysAuthorization>();
            SysUserGroup = new HashSet<SysUserGroup>();
        }

        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmployeeId { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public bool? RefuseEmail { get; set; }
        public Guid? LdapObjectGuid { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual ICollection<SysAuthorization> SysAuthorizationAssignToNavigation { get; set; }
        public virtual ICollection<SysAuthorization> SysAuthorizationUser { get; set; }
        public virtual ICollection<SysUserGroup> SysUserGroup { get; set; }
    }
}
