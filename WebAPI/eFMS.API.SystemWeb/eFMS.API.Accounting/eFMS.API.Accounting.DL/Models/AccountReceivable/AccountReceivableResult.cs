using System;

namespace eFMS.API.Accounting.DL.Models.AccountReceivable
{
    public class AccountReceivableResult
    {
        public Guid? AgreementId { get; set; }
        public string PartnerId { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerNameEn { get; set; }
        public string PartnerNameLocal { get; set; }
        public string PartnerNameAbbr { get; set; }
        public string TaxCode { get; set; }
        public string PartnerStatus { get; set; }
        public string AgreementNo { get; set; }
        public string AgreementType { get; set; }
        public string AgreementStatus { get; set; }
        public string AgreementSalesmanId { get; set; }
        public string AgreementSalesmanName { get; set; }
        public string AgreementCurrency { get; set; }
        public string OfficeId { get; set; }
        public string ArServiceCode { get; set; }
        public string ArServiceName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpriedDate { get; set; }
        public int? ExpriedDay { get; set; }
        public decimal? CreditLimited { get; set; }
        public int? CreditTerm { get; set; }
        public decimal? CreditRateLimit { get; set; }
        public decimal? SaleCreditLimited { get; set; }
        public decimal? SaleDebitAmount { get; set; }
        public decimal? SaleDebitRate { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? ObhAmount { get; set; }
        public decimal? DebitRate { get; set; }
        public decimal? CusAdvance { get; set; }
        public decimal? BillingAmount { get; set; }
        public decimal? BillingUnpaid { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? Over1To15Day { get; set; }
        public decimal? Over16To30Day { get; set; }
        public decimal? Over30Day { get; set; }
        public string ArCurrency { get; set; }
    }
}
