using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctSOADetailResult : AcctSOAResult
    {
        public List<GroupShipmentModel> GroupShipments { get; set; }
        public List<ChargeShipmentModel> ChargeShipments { get; set; }
        public int TotalCharge { get; set; }
        public decimal AmountDebitLocal { get; set; }
        public decimal AmountCreditLocal { get; set; }
        public decimal AmountBalanceLocal { get { return this.AmountDebitLocal - this.AmountCreditLocal; } }
        public decimal AmountDebitUSD { get; set; }
        public decimal AmountCreditUSD { get; set; }
        public decimal AmountBalanceUSD { get { return this.AmountDebitUSD - this.AmountCreditUSD; } }
        public string ServicesNameSoa { get; set; }
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
        public string CreditPayment { get; set; }
        public bool IsExistCurrencyDiffLocal { get; set; }
    }
}
