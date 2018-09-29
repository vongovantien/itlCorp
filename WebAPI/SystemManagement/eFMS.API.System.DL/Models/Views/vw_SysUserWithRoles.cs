using System;
using System.Collections.Generic;
using System.Text;
using SystemManagementAPI.Service.Models;
namespace SystemManagement.DL.Models.Views
{
    public class vw_SysUserWithRoles
    {
        public vw_sysUser sysUser { get; set; }
        public List<vw_SysUserRole> listRole { get; set; } 
    }
}
