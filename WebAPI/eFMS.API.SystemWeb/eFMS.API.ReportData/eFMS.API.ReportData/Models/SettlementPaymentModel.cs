using System;

namespace eFMS.API.ReportData.Models
{
    public class SettlementPaymentModel
    {
        public int No { get; set; }
        public string SettlementNo { get; set; }
        public decimal Amount { get; set; }
        public string SettlementCurrency { get; set; }
        public string RequesterName { get; set; }
        public DateTime? RequestDate { get; set; }
        public string StatusApprovalName { get; set; }
        public string PaymentMethodName { get; set; }
        public string Note { get; set; }
    }
}
