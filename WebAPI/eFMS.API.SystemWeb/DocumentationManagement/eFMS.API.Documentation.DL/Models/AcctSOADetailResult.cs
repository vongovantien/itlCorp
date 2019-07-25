using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class AcctSOADetailResult : AcctSOAResult
    {
        public List<ChargeShipmentModel> ChargeShipments { get; set; }
        public decimal AmountDebitLocal { get; set; }
        public decimal AmountCreditLocal { get; set; }
        public decimal AmountBalanceLocal { get { return this.AmountDebitLocal - this.AmountCreditLocal; } }
        public decimal AmountDebitUSD { get; set; }
        public decimal AmountCreditUSD { get; set; }
        public decimal AmountBalanceUSD { get { return this.AmountDebitUSD - this.AmountCreditUSD; } }
    }
}
