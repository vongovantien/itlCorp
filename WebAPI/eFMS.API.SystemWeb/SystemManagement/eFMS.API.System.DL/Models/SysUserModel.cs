using eFMS.API.System.Service.Models;

namespace eFMS.API.System.DL.Models
{
    public class SysUserModel : SysUser
    {
        public SysEmployeeModel SysEmployeeModel { get; set; }
    }
}
