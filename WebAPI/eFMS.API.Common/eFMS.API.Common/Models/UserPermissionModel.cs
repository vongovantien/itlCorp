using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Common.Models
{
    public class UserPermissionModel
    {
        public UserPermissionModel()
        {
            SpecialActions = new List<SpecialAction>();
        }
        public string MenuId { get; set; }
        public bool? Access { get; set; }
        public string Detail { get; set; }
        public string Write { get; set; }
        public string Delete { get; set; }
        public string List { get; set; }
        public bool? Import { get; set; }
        public bool? Export { get; set; }
        public bool AllowAdd { get; set; }
        public List<SpecialAction> SpecialActions { get; set; }
    }
    public class SpecialAction
    {
        public string Action { get; set; }
        public bool? IsAllow { get; set; }
    }
}
