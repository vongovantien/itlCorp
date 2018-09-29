using System;
using System.Collections.Generic;
using System.Text;

namespace SystemManagement.DL.Models.Views
{
    public partial class vw_sysUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public short UserGroupID { get; set; }
        public string UserGroupCode { get; set; }
        public string UserGroupName { get; set; }
        public System.Guid WorkPlaceID { get; set; }
        public string WorkPlaceCode { get; set; }
        public Nullable<System.Guid> LdapObjectGuid { get; set; }
        public Nullable<bool> RefuseEmail { get; set; }
        public string UserCreated { get; set; }
        public Nullable<System.DateTime> DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<System.DateTime> DatetimeModified { get; set; }
        public bool Inactive { get; set; }
        public Nullable<System.DateTime> InactiveOn { get; set; }
        public string SugarID { get; set; }
        public string ID { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeName_VN { get; set; }
        public string EmployeeName_EN { get; set; }
        public string PlaceTypeName_VN { get; set; }
    }
}
