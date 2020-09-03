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
        public string Status { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string StaffCode { get; set; }
        public string EmployeeNameVn { get; set; }
        public string EmployeeNameEn { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
        public string Title { get; set; }
        public string WorkingStatus { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? CreditRate { get; set; }
    }
}
