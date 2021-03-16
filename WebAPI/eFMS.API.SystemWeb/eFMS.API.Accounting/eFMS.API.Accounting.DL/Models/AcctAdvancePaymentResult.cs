using eFMS.API.Accounting.Service.Models;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctAdvancePaymentResult : AcctAdvancePayment
    {
        public string RequesterName { get; set; }
        public string AdvanceStatusPayment { get; set; }
        public decimal? Amount { get; set; }
        public string StatusApprovalName { get; set; }
        public string PaymentMethodName { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
        public string PayeeName { get; set; }

    }
}
