using System;

namespace eFMS.API.ReportData.Models.Accounting
{
    public class InfoSettlementExport
    {
        public string Requester { get; set; }
        public DateTime? RequestDate { get; set; }
        public string Department { get; set; }
        public string SettlementNo { get; set; }
        public decimal? SettlementAmount { get; set; }
        public string SettlementCurrency { get; set; }
        public string PaymentMethod { get; set; }
        public string Manager { get; set; }
        public string Accountant { get; set; }
        public bool IsRequesterApproved { get; set; }
        public bool IsManagerApproved { get; set; }
        public bool IsAccountantApproved { get; set; }
        public bool IsBODApproved { get; set; }
        public string ContactOffice { get; set; }
        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string PayeeName { get; set; }
        public string AmountInWords { get; set; }
        public string Note { get; set; }
    }
}
