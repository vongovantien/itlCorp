using System;

namespace eFMS.API.Accounting.DL.Models.ExportResults
{
    public class InfoAdvanceExport
    {
        public string Requester { get; set; }
        public DateTime? RequestDate { get; set; }
        public string Department { get; set; }
        public string AdvanceNo { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public string AdvanceAmountWord { get; set; }
        public string AdvanceReason { get; set; }
        public DateTime? DealinePayment { get; set; }
        public string Manager { get; set; }
        public string Accountant { get; set; }
        public bool IsRequesterApproved { get; set; }
        public bool IsManagerApproved { get; set; }
        public bool IsAccountantApproved { get; set; }
        public bool IsBODApproved { get; set; }
        public string ContactOffice { get; set; }
    }
}
