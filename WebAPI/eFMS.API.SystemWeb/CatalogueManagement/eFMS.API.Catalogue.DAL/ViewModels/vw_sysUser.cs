using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.Service.ViewModels
{
    public partial class vw_sysUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string UserGroupCode { get; set; }
        public string UserGroupName { get; set; }
        public Guid WorkPlaceID { get; set; }
        public string WorkPlaceCode { get; set; }
        public Nullable<Guid> LdapObjectGuid { get; set; }
        public Nullable<bool> RefuseEmail { get; set; }
        public string UserCreated { get; set; }
        public Nullable<DateTime> DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<DateTime> DatetimeModified { get; set; }
        public bool Active { get; set; }
        public Nullable<DateTime> ActiveOn { get; set; }
        public string ID { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeName_VN { get; set; }
        public string EmployeeName_EN { get; set; }
    }
}
