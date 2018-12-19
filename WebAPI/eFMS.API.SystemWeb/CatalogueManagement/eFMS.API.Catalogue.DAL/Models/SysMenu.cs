using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class SysMenu
    {
        public SysMenu()
        {
            InverseParent = new HashSet<SysMenu>();
            SysRoleMenu = new HashSet<SysRoleMenu>();
            SysRolePermission = new HashSet<SysRolePermission>();
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
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public SysMenu Parent { get; set; }
        public ICollection<SysMenu> InverseParent { get; set; }
        public ICollection<SysRoleMenu> SysRoleMenu { get; set; }
        public ICollection<SysRolePermission> SysRolePermission { get; set; }
    }
}
