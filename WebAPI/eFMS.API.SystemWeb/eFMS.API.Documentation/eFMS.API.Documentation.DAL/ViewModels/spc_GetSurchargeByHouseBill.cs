using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class spc_GetSurchargeByHouseBill
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
        public bool? KickBack { get; set; }
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
        public bool? IsFromShipment { get; set; }
        public string PaySoano { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string ChargeNameEn { get; set; }
        public string ChargeCode { get; set; }
        public string UnitNameEn { get; set; }
        public string UnitCode { get; set; }
        public string PartnerName { get; set; }
        public string ReceiverName { get; set; }
        public string PayerName { get; set; }
        public string CurrencyCode { get; set; }
        public decimal RateToLocal { get; set; }
        public decimal RateToUSD { get; set; }
        public string PartnerShortName { get; set; }
        public string ReceiverShortName { get; set; }
        public string PayerShortName { get; set; }
        public Decimal? FinalExchangeRate { get; set; }
        public string AdvanceNo { get; set; }
        public string JobNo { get; set; }
        public string MBLNo { get; set; }
        public string HBLNo { get; set; }
        public string VoucherId { get; set; }
        public DateTime? VoucherIddate { get; set; }
        public string VoucherIdre { get; set; }
        public DateTime? VoucherIdredate { get; set; }
        public Guid? AcctManagementId { get; set; }
    }
}
