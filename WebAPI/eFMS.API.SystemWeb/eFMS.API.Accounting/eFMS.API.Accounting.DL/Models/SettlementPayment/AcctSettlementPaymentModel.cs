using eFMS.API.Accounting.Service.Models;

namespace eFMS.API.Accounting.DL.Models.SettlementPayment
{
    public class AcctSettlementPaymentModel : AcctSettlementPayment
    {
        public int NumberOfRequests { get; set; }
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
    }
}
