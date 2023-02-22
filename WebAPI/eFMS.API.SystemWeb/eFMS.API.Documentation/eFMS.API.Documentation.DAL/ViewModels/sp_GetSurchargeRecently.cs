using eFMS.API.Documentation.Service.Models;
using System;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class sp_GetSurchargeRecently
    {
        public string Type { get; set; }
        public int UnitId { get; set; }
        public Guid ChargeId { get; set; }
        public string ChargeNameEn { get; set; }
        public string ChargeCode { get; set; }
        public decimal? Quantity { get; set; }
        public string QuantityType { get; set; }
        public decimal? UnitPrice { get; set; }
        public string CurrencyId { get; set; }
        public decimal? Vatrate { get; set; }
        public decimal? Total { get; set; }
        public string PaymentObjectId { get; set; }
        public DateTime? ExchangeDate { get; set; }
        public string Notes { get; set; }
        public bool IsFromShipment { get; set; }
        public bool? IsRefundFee { get; set; }
        public bool KickBack { get; set; }
        public Guid? ChargeGroup { get; set; }
        public string PartnerShortName { get; set; }
        public string PartnerName { get; set; }
    }
}
