using eFMS.API.System.Service.Models;
using System;

namespace eFMS.API.System.DL.Models
{
    public class SysEmailSettingModel : SysEmailSetting
    {
        public string EmailType { get; set; }
        public string EmailInfo { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int DeptId { get; set; }
        public string UserModified { get; set; }
        public string UserCreated { get; set; }
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
    }
}
