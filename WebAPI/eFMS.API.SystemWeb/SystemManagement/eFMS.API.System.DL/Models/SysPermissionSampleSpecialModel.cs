using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class SysPermissionSampleSpecialModel: SysPermissionSampleSpecial
    {
        public string MenuName { get; set; }
    }
    public class SysPermissionSpecialViewModel
    {
        public short PermissionId { get; set; }
        public string ModuleId { get; set; }
        public string MenuId { get; set; }
        public string MenuName { get; set; }
        public List<PermissionSpecialAction> PermissionSpecialActions { get; set; }
    }

    public class PermissionSpecialAction
    {
        public short PermissionId { get; set; }
        public short Id { get; set; }
        public string ModuleId { get; set; }
        public string MenuId { get; set; }
        public string NameEn { get; set; }
        public string NameVn { get; set; }
        public bool? IsAllow { get; set; }
    }
}
