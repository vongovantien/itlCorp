using System;

namespace eFMS.API.Accounting.DL.Models
{
    public class ShipmentOfSettlementResult
    {
        public string JobId { get; set; }
        public string MBL { get; set; }
        public string HBL { get; set; }
        public decimal Amount { get; set; }
        public string ChargeCurrency { get; set; }
        public string SettlementCurrency { get; set; }
        public bool? IsLocked { get; set; }
    }
}
