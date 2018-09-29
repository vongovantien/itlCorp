using System;
using System.Collections.Generic;
using System.Text;

namespace SystemManagement.DL.Models.Views
{
    public partial class vw_SysUserRole
    {
        public string UserID { get; set; }
        public short RoleID { get; set; }
        public int UserRoleId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string UserModified { get; set; }
        public Nullable<System.DateTime> DatetimeModified { get; set; }
        public bool Inactive { get; set; }
        public System.Guid WorkPlaceID { get; set; }
        public string WorkPlaceCode { get; set; }
        public string UserModifiedVN { get; set; }
        public string UserModifiedEN { get; set; }
        public Nullable<System.DateTime> InactiveOn { get; set; }
    }
}
