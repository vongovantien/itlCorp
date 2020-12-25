using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsShipmentSurchargeDetailsModel : CsShipmentSurchargeModel
    {
        public string PartnerName { get; set; }
        public string NameEn { get; set; }
        public string ReceiverName { get; set; }
        public string ChargeNameEn { get; set; }
        public string PayerName { get; set; }
        public string Unit { get; set; }
        public string UnitCode { get; set; }
        public string Currency { get; set; }
        public string ChargeCode { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? ExchangeRateUSDToVND { get; set; }
        public decimal RateToUSD { get; set; }
        public string Hwbno { get; set; }
        public string PartnerShortName { get; set; }
        public string ReceiverShortName { get; set; }
        public string PayerShortName { get; set; }
        public Guid? DebitCharge { get; set; }
        public bool IsSynced { get; set; }
        public string SyncedFromBy { get; set; }
    }
}
