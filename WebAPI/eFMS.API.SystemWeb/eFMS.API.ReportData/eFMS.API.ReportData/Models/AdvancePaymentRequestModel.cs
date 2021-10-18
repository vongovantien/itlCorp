using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class AdvancePaymentRequestModel
    {
        public DateTime? RequestDate { get; set; }
        public DateTime? ApproveDate { get; set; }
        public DateTime? SettleDate { get; set; }
        public string Requester { get; set; }
        public string Description { get; set; }
        public string CustomNo { get; set; }
        public string JobId { get; set; }
        public string Hbl { get; set; }
        public string Mbl { get; set; }
        public decimal? Amount { get; set; }
        public string RequestCurrency { get; set; }
        public string AdvanceType { get; set; }
        public string RequestNote { get; set; }
        public string StatusPayment { get; set; }
        public string AdvanceNo { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime DeadlinePayment { get; set; }
        public string BankAccountNo { get; set; }
        public string BankAccountName { get; set; }
        public string BankName { get; set; }
    }
}
