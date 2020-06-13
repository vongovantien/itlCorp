using System;

namespace eFMS.API.Accounting.DL.Models
{
    public class ChargeOfAccountingManagementModel
    {
        public Guid SurchargeId { get; set; }
        public Guid ChargeId { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string JobNo { get; set; }
        public string Hbl { get; set; }
        public string ContraAccount { get; set; }
        public decimal? OrgAmount { get; set; }
        public decimal? Vat { get; set; }
        public decimal? OrgVatAmount { get; set; }
        public string VatAccount { get; set; }
        public string Currency { get; set; }
        public DateTime? ExchangeDate { get; set; }
        public decimal? FinalExchangeRate { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? AmountVnd { get; set; }
        public decimal? VatAmountVnd { get; set; }
        public string VatPartnerId { get; set; }
        public string VatPartnerCode { get; set; }
        public string VatPartnerName { get; set; }
        public string VatPartnerAddress { get; set; }
        public string ObhPartnerCode { get; set; }
        public string ObhPartner { get; set; }
        public string InvoiceNo { get; set; }
        public string Serie { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string CdNoteNo { get; set; }
        public decimal? Qty { get; set; }
        public string UnitName { get; set; }
        public decimal? UnitPrice { get; set; }
        public string Mbl { get; set; }
        public string SoaNo { get; set; }
        public string SettlementCode { get; set; }
        public Guid? AcctManagementId { get; set; }
        public string RequesterId { get; set; }
    }
}
