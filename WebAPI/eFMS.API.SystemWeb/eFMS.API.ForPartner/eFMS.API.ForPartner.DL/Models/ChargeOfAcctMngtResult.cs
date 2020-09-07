using System;

namespace eFMS.API.ForPartner.DL.Models
{
    public class ChargeOfAcctMngtResult
    {
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string JobNo { get; set; }
        public string Hbl { get; set; }
        public string Mbl { get; set; }
        public string CustomNo { get; set; }
        public decimal? Qty { get; set; }
        public string UnitName { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? OrgAmount { get; set; }
        public decimal? Vat { get; set; }
        public decimal? OrgVatAmount { get; set; }
        public string Currency { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string VatPartnerId { get; set; }
        public string VatPartnerName { get; set; }
        public string DebitNo { get; set; }
        public string SoaNo { get; set; }
        public string InvoiceNo { get; set; }
        public string Serie { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? ExchangeDate { get; set; }
        public decimal? FinalExchangeRate { get; set; }
        public string PartnerMode { get; set; }
        public string PartnerLocation { get; set; }
        public string PartnerReference { get; set; }

    }
}
