using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.Service.Models
{
    public partial class SysActionFuncLog
    {
        public Guid Id { get; set; }
        public string FuncLocal { get; set; }
        public string FuncPartner { get; set; }
        public string ObjectRequest { get; set; }
        public string ObjectResponse { get; set; }
        public string Major { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public DateTime? StartDateProgress { get; set; }
        public DateTime? EndDateProgress { get; set; }
    }
}
