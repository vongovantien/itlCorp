using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models.Criteria
{
    public class SysPermissionGeneralCriteria
    {
        public string All { get; set; }
        public string Name { get; set; }
        public short? RoleId { get; set; }
        public bool? Active { get; set; }
    }
}
