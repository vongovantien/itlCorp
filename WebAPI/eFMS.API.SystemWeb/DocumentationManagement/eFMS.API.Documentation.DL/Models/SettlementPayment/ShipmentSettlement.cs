using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.SettlementPayment
{
    public class ShipmentSettlement
    {
        public string SettlementNo { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyShipment { get; set; }
        public List<ChargeSettlement> ChargeSettlements { get; set; }
    }
}
