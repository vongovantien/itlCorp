using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class AccountingPlSheetExportResult
    {
        public Guid? Hblid { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string JobId { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerTaxCode { get; set; }
        public string PartnerName { get; set; }
        public string Mbl { get; set; }
        public string Hbl { get; set; }
        public Guid? Pol { get; set; }
        public string CustomNo { get; set; }
        public string PaymentMethodTerm { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string TaxInvNoRevenue { get; set; }
        public string VoucherIdRevenue { get; set; }
        public decimal? UsdRevenue { get; set; }
        public decimal? VndRevenue { get; set; }
        public decimal? TaxOut { get; set; }
        public decimal? TotalRevenue { get; set; }
        public string TaxInvNoCost { get; set; }
        public string VoucherIdCost { get; set; }
        public decimal? UsdCost { get; set; }
        public decimal? VndCost { get; set; }
        public decimal? TaxIn { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? TotalKickBack { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal? Balance { get; set; }
        public string InvNoObh { get; set; }
        public decimal? AmountObh { get; set; }
        public DateTime? PaidDate { get; set; }
        public string AcVoucherNo { get; set; }
        public string PmVoucherNo { get; set; }
        public string Service { get; set; }
        public string UserExport { get; set; }
        public decimal? FinalExchangeRate { get; set; }
        public DateTime? ExchangeDate { get; set; }
        public string CurrencyId { get; set; }
        public decimal? AmountUSD { get; set; }
        public decimal? AmountVND { get; set; }
        public decimal? VATAmountUSD { get; set; }
        public decimal? VATAmountVND { get; set; }
        public string CdNote { get; set; }
    }
}
