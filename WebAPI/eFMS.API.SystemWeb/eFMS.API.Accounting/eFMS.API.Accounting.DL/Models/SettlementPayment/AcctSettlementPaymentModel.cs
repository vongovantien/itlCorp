using eFMS.API.Accounting.Service.Models;

namespace eFMS.API.Accounting.DL.Models.SettlementPayment
{
    public class AcctSettlementPaymentModel : AcctSettlementPayment
    {
        public int NumberOfRequests { get; set; }
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
        public bool IsRequester { get; set; }
        public bool IsManager { get; set; }
        public bool IsApproved { get; set; }
        public bool IsShowBtnDeny { get; set; }
    }
}
