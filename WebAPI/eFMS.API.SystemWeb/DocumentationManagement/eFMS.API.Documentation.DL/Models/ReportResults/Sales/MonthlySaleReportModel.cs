using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.ReportResults.Sales
{
    public class MonthlySaleReportModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string DateType { get; set; }
        public string CustomerId { get; set; }
        public string ServiceType { get; set; }
        public string CurrencyId { get; set; }
        public string RefNo { get; set; }
        public string RefType { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? DepartmentId { get; set; }
        public int? GroupId { get; set; }
        public string UserId { get; set; }
        public string UserType { get; set; }
        public string CarrierId { get; set; }
        public string AgentId { get; set; }
        public Guid? POL { get; set; }
        public Guid? POD { get; set; }
    }
}
