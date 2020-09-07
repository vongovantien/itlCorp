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
        public decimal TotalAmount { get; set; }
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
