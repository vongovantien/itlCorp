using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class SysPermissionSpecialAction
    {
        public short Id { get; set; }
        public string ModuleId { get; set; }
        public string MenuId { get; set; }
        public string ActionName { get; set; }
        public string NameEn { get; set; }
        public string NameVn { get; set; }
    }
}
