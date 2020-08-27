using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CsShipmentSurcharge
    {
        public Guid Id { get; set; }
        public Guid Hblid { get; set; }
        public string Type { get; set; }
        public Guid ChargeId { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityType { get; set; }
        public short UnitId { get; set; }
        public decimal? UnitPrice { get; set; }
        public string CurrencyId { get; set; }
        public bool? IncludedVat { get; set; }
        public decimal? Vatrate { get; set; }
        public decimal Total { get; set; }
        public string PayerId { get; set; }
        public string ObjectBePaid { get; set; }
        public string PaymentObjectId { get; set; }
        public DateTime? ExchangeDate { get; set; }
        public string Notes { get; set; }
        public string SettlementCode { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string SeriesNo { get; set; }
        public string PaymentRequestType { get; set; }
        public string ClearanceNo { get; set; }
        public string ContNo { get; set; }
        public string PaymentRefNo { get; set; }
        public string Status { get; set; }
        public bool? Soaclosed { get; set; }
        public bool? Cdclosed { get; set; }
        public string CreditNo { get; set; }
        public string DebitNo { get; set; }
        public string Soano { get; set; }
        public string PaySoano { get; set; }
        public bool? IsFromShipment { get; set; }
        public string TypeOfFee { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string VoucherId { get; set; }
        public DateTime? VoucherIddate { get; set; }
        public string VoucherIdre { get; set; }
        public DateTime? VoucherIdredate { get; set; }
        public decimal? FinalExchangeRate { get; set; }
        public bool? KickBack { get; set; }
        public string AdvanceNo { get; set; }
        public string JobNo { get; set; }
        public string Mblno { get; set; }
        public string Hblno { get; set; }
        public Guid? AcctManagementId { get; set; }
        public Guid? ChargeGroup { get; set; }
        public string TransactionType { get; set; }
        public Guid? OfficeId { get; set; }
    }
}
