using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class SysGroupModel: SysGroup
    {
        public string DepartmentName { get; set; }
        public string CompanyName { get; set; }
        public string OfficeName { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? OfficeId { get; set; }
        public string NameUserCreated { get; set; }
        public string NameUserModified { get; set; }

    }
}
