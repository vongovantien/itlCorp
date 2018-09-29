using System;
using System.Collections.Generic;
using System.Text;

namespace SystemManagement.DL.Models.Views
{
    public partial class vw_sysEmployee
    {
        public string ID { get; set; }
        public System.Guid WorkPlaceID { get; set; }
        public string WorkPlaceDisplayName { get; set; }
        public string DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeName_VN { get; set; }
        public string EmployeeName_EN { get; set; }
        public string Position { get; set; }
        public string PositionName { get; set; }
        public Nullable<System.DateTime> Birthday { get; set; }
        public string ExtNo { get; set; }
        public string Tel { get; set; }
        public string HomePhone { get; set; }
        public string HomeAddress { get; set; }
        public string Email { get; set; }
        public string AccessDescription { get; set; }
        public byte[] Photo { get; set; }
        public string EmpPhotoSize { get; set; }
        public Nullable<decimal> SaleTarget { get; set; }
        public Nullable<decimal> Bonus { get; set; }
        public string SaleResource { get; set; }
        public byte[] Signature { get; set; }
        public Nullable<System.Guid> LdapObjectGuid { get; set; }
        public string UserCreated { get; set; }
        public Nullable<System.DateTime> DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<System.DateTime> DatetimeModified { get; set; }
        public Nullable<bool> Inactive { get; set; }
        public Nullable<System.DateTime> InactiveOn { get; set; }
    }
}
