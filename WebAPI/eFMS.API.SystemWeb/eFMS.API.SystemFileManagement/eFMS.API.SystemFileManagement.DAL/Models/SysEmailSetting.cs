using System;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.Service.Models
{
    public partial class SysEmailSetting
    {
        public int Id { get; set; }
        public string EmailType { get; set; }
        public string EmailInfo { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int DeptId { get; set; }
        public string UserModified { get; set; }
        public string UserCreated { get; set; }
    }
}
