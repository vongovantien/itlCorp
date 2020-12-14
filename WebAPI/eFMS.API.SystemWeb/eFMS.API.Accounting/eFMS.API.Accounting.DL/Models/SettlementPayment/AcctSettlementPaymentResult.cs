using eFMS.API.Accounting.Service.Models;

namespace eFMS.API.Accounting.DL.Models.SettlementPayment
{
    public class AcctSettlementPaymentResult : AcctSettlementPayment
    {
        public decimal Amount { get; set; }
        public string RequesterName { get; set; }
        public string PaymentMethodName { get; set; }        
        public string StatusApprovalName { get; set; }
        public string ChargeCurrency { get; set; }
        public string PayeeName { get; set; }
    }
}
