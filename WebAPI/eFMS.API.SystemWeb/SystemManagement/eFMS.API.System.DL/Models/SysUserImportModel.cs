using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class SysUserImportModel: SysEmployeeModel
    {
        public string Username { get; set; }
        public string UserType { get; set; }
        public bool IsValid { get; set; }

    }
}
