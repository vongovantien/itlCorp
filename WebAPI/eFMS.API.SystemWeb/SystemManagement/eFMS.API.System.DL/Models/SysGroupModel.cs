using eFMS.API.System.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class SysGroupModel: SysGroup
    {
        //public short Id { get; set; }
        //public string Code { get; set; }
        //public string NameEn { get; set; }
        //public string NameVn { get; set; }
        //public int? DepartmentId { get; set; }
        //public short ParentId { get; set; }
        //public string ManagerId { get; set; }
        //public string ShortName { get; set; }
        //public string UserCreated { get; set; }
        //public DateTime? DatetimeCreated { get; set; }
        //public string UserModified { get; set; }
        //public DateTime? DatetimeModified { get; set; }
        //public bool? Active { get; set; }
        //public DateTime? InactiveOn { get; set; }
        public string DepartmentName { get; set; }
        public string CompanyName { get; set; }
        public string OfficeName { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? OfficeId { get; set; }
    }
}
