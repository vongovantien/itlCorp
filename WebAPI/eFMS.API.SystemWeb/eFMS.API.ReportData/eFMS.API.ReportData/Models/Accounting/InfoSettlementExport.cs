using System;

namespace eFMS.API.ReportData.Models.Accounting
{
    public class InfoSettlementExport
    {
        public string Requester { get; set; }
        public DateTime? RequestDate { get; set; }
        public string Department { get; set; }
        public string SettlementNo { get; set; }
        public string Manager { get; set; }
        public string Accountant { get; set; }
    }
}
