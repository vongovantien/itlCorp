using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.DL.Models
{
    public class PermissionInfo
    {
        public Guid? CompanyID { get; set; }
        public Guid? OfficeID { get; set; }
        public short? DepartmentID { get; set; }
        public int? GroupID { get; set; }
    }
}
