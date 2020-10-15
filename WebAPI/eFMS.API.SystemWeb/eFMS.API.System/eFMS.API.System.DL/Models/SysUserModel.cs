using eFMS.API.System.Service.Models;

namespace eFMS.API.System.DL.Models
{
    public class SysUserModel : SysUser
    {
        public string EmployeeNameVn { get; set; }

        public SysEmployeeModel SysEmployeeModel { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }

        public string Avatar { get; set; }
    }
}
