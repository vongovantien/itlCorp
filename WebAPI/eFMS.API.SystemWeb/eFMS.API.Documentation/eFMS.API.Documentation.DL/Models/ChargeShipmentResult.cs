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

    public class AmountSurchargeResult
    {
        public decimal NetAmountOrig { get; set; } //Tiền trước thuế (Original)
        public decimal VatAmountOrig { get; set; } //Tiền thuế (Original)
        public decimal GrossAmountOrig { get; set; } //Tiền sau thuế (Original)
        public decimal FinalExchangeRate { get; set; }
        public decimal AmountVnd { get; set; }
        public decimal VatAmountVnd { get; set; }
        public decimal AmountUsd { get; set; }
        public decimal VatAmountUsd { get; set; }
    }
}
