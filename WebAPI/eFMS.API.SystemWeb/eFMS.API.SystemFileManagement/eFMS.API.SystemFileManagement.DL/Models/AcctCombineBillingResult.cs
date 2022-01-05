using eFMS.API.SystemFileManagement.Service.Models;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class AcctCombineBillingResult : AcctCombineBilling
    {
        public string JobNo { get; set; }
        public string Soano { get; set; }
        public string PaySoano { get; set; }
        public string CreditNo { get; set; }
        public string DebitNo { get; set; }
        public string PartnerName { get; set; }
        public string UserCreatedName { get; set; }
    }
}
