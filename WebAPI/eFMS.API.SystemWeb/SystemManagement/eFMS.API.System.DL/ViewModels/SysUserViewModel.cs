using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.ViewModels
{
    public class SysUserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public short UserGroupId { get; set; }
        public string EmployeeId { get; set; }
        public Guid WorkPlaceId { get; set; }
        public bool? RefuseEmail { get; set; }
        public Guid? LdapObjectGuid { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }

        public string EmployeeNameVn { get; set; }
        public string EmployeeNameEn { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
        public string Title { get; set; }
    }
}
