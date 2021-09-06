using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.ViewModels
{
    public class sp_GetDataExistsCharge : ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public Guid Id { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string ChargeCode { get; set; }
        public Guid Hblid { get; set; }
        public string Type { get; set; }
        public Guid ChargeId { get; set; }
        public string ChargeName { get; set; }
        public decimal Quantity { get; set; }
        public short UnitId { get; set; }
        public string UnitName { get; set; }
        public decimal? UnitPrice { get; set; }
        public string CurrencyId { get; set; }
        public decimal? Vatrate { get; set; }
        public decimal Total { get; set; }
        public string PayerId { get; set; }
        public string Payer { get; set; }
        public string PaymentObjectId { get; set; }
        public string OBHPartnerName { get; set; }
        public string Notes { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string SeriesNo { get; set; }
        public string ClearanceNo { get; set; }
        public string ContNo { get; set; }
        public bool? IsFromShipment { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? FinalExchangeRate { get; set; }
        public decimal? AmountVnd { get; set; }
        public decimal? VatAmountVnd { get; set; }
        public decimal? AmountUSD { get; set; }
        public decimal? VatAmountUSD { get; set; }
        public decimal? TotalAmountVnd { get; set; }
        public string PICName { get; set; }
        public bool? KickBack { get; set; }
        public string VatPartnerShortName { get; set; }
        public string VatPartnerId { get; set; }
    }
}
