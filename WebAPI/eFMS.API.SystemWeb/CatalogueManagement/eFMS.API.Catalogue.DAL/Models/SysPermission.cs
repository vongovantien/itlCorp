using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class SysPermission
    {
        public SysPermission()
        {
            SysRolePermission = new HashSet<SysRolePermission>();
        }

        public short Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        public DateTime? ActiveOn { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? RequireAccessingForm { get; set; }

        public virtual ICollection<SysRolePermission> SysRolePermission { get; set; }
    }
}
