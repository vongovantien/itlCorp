using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class AdvancePaymentModel
    {
        public int No { get; set; }
        public string AdvanceNo { get; set; }
        public decimal Amount { get; set; }
        public string AdvanceCurrency { get; set; }
        public string RequesterName { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DeadlinePayment { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string StatusApprovalName { get; set; }
        public string AdvanceStatusPayment { get; set; }
        public string PaymentMethodName { get; set; }
        public string AdvanceNote { get; set; }
    }
}
