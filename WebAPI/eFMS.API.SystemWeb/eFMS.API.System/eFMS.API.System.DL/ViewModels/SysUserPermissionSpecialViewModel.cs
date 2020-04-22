using eFMS.API.System.DL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.ViewModels
{
    public class SysUserPermissionSpecialViewModel
    {
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string MenuId { get; set; }
        public string MenuName { get; set; }
        public Guid UserPermissionId { get; set; }
        public List<UserPermissionSpecialViewModel> SysPermissionSpecials { get; set; }
    }
    public class UserPermissionSpecialViewModel
    {
        public Guid UserPermissionId { get; set; }
        public string ModuleId { get; set; }
        public string MenuId { get; set; }
        public string MenuName { get; set; }
        public List<UserPermissionSpecialAction> PermissionSpecialActions { get; set; }
    }
    public class UserPermissionSpecialAction
    {
        public Guid UserPermissionId { get; set; }
        public Guid Id { get; set; }
        public string ModuleId { get; set; }
        public string MenuId { get; set; }
        public string NameEn { get; set; }
        public string NameVn { get; set; }
        public string ActionName { get; set; }
        public bool? IsAllow { get; set; }
    }
}
