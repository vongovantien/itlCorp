using eFMS.API.System.Service.Models;
using System;

namespace eFMS.API.System.DL.Models
{
    public class SysEmailSettingModel : SysEmailSetting
    {
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
    }
}
