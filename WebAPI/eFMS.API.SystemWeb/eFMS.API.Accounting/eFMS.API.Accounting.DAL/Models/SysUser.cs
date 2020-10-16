using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class SysUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordLdap { get; set; }
        public bool? IsLdap { get; set; }
        public string EmployeeId { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public bool? RefuseEmail { get; set; }
        public Guid? LdapObjectGuid { get; set; }
        public string UserType { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string WorkingStatus { get; set; }
        public string Description { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? CreditRate { get; set; }
        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
    }
}
