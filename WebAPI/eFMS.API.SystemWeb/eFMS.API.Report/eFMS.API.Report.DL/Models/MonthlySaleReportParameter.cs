using System;

namespace eFMS.API.Report.DL.Models
{
    public class MonthlySaleReportParameter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Contact { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress1 { get; set; }
        public decimal CurrDecimalNo { get; set; }
        public string ReportBy { get; set; }
        public string Director { get; set; }
        public string ChiefAccountant { get; set; }
    }
}
