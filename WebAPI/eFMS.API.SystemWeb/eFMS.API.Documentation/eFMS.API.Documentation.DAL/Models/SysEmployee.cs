using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysEmployee
    {
        public string Id { get; set; }
        public Guid CompanyId { get; set; }
        public string DepartmentId { get; set; }
        public string EmployeeNameVn { get; set; }
        public string EmployeeNameEn { get; set; }
        public string Position { get; set; }
        public string Title { get; set; }
        public DateTime? Birthday { get; set; }
        public string ExtNo { get; set; }
        public string Tel { get; set; }
        public string HomePhone { get; set; }
        public string HomeAddress { get; set; }
        public string Email { get; set; }
        public string AccessDescription { get; set; }
        public byte[] Photo { get; set; }
        public string EmpPhotoSize { get; set; }
        public decimal? SaleTarget { get; set; }
        public decimal? Bonus { get; set; }
        public string StaffCode { get; set; }
        public string SaleResource { get; set; }
        public Guid? LdapObjectGuid { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public byte[] Signature { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
