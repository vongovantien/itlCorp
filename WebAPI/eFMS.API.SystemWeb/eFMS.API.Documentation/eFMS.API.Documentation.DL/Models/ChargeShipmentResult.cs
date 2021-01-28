using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class ChargeShipmentResult
    {
        public List<ChargeShipmentModel> ChargeShipments { get; set; }
        public int TotalShipment { get; set; }
        public int TotalCharge { get; set; }
        public decimal AmountDebitLocal { get; set; }
        public decimal AmountCreditLocal { get; set; }
        public decimal AmountBalanceLocal { get { return this.AmountDebitLocal - this.AmountCreditLocal; } }
        public decimal AmountDebitUSD { get; set; }
        public decimal AmountCreditUSD { get; set; }
        public decimal AmountBalanceUSD { get { return this.AmountDebitUSD - this.AmountCreditUSD; } }
    }

    public class AmountResult
    {
        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
