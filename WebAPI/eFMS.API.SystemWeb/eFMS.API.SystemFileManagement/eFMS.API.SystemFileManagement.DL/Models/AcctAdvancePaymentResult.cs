using eFMS.API.SystemFileManagement.Service.Models;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class AcctAdvancePaymentResult : AcctAdvancePayment
    {
        public string RequesterName { get; set; }
        public string AdvanceStatusPayment { get; set; }
        public decimal? Amount { get; set; }
        public string StatusPayment { get; set; }
        public string StatusApprovalName { get; set; }
        public string PaymentMethodName { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
        public string PayeeName { get; set; }      
        public string DepartmentName { get; set; }
    }
}
