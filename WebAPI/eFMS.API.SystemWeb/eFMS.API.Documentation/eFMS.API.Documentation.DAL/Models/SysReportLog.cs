using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysReportLog
    {
        public Guid Id { get; set; }
        public string ReportName { get; set; }
        public string ObjectParameter { get; set; }
        public string ObjectResult { get; set; }
        public string Type { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
