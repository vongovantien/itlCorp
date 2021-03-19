using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.SettlementPayment
{
    public class ShipmentSettlement
    {
        public string SettlementNo { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        // Total Org netamount USD
        public decimal TotalNetAmount { get; set; }
        // Total Org netamount VND
        public decimal TotalNetAmountVND { get; set; }
        // Total Org amount USD
        public decimal TotalAmount { get; set; }
        // Total Org amount VND
        public decimal TotalAmountVND { get; set; }
        // Total net amount VND after exchange
        public decimal TotalNetVND { get; set; }
        // Total VAT amount VND after exchange
        public decimal TotalVATVND { get; set; }
        // Total net amount USD after exchange
        public decimal TotalNetUSD { get; set; }
        // Total VAT amount USD after exchange
        public decimal TotalVATUSD { get; set; }
        // Total amount VND after exchange
        public decimal TotalVND { get; set; }
        public string CurrencyShipment { get; set; }
        public Guid HblId { get; set; }
        public Guid ShipmentId { get; set; }
        public string Type { get; set; }
        public string CustomNo { get; set; }
        public string AdvanceNo { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public decimal? Balance { get; set; }
        public List<ShipmentChargeSettlement> ChargeSettlements { get; set; }
    }
}
