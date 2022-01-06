using System;
using System.Collections.Generic;

namespace eFMS.IdentityServer.Service.Models
{
    public partial class SysMenu
    {
        public SysMenu()
        {
            InverseParent = new HashSet<SysMenu>();
        }

        public string Id { get; set; }
        public string ParentId { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public string AssemplyName { get; set; }
        public string Icon { get; set; }
        public int? Sequence { get; set; }
        public string Arguments { get; set; }
        public string Route { get; set; }
        public bool? DisplayChild { get; set; }
        public bool? Display { get; set; }
        public int? OrderNumber { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual SysMenu Parent { get; set; }
        public virtual ICollection<SysMenu> InverseParent { get; set; }
    }
}
