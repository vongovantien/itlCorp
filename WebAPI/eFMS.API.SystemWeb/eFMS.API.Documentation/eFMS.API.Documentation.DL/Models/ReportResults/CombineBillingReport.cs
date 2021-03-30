using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class CombineBillingReport
    {
        public string PartnerID { get; set; }
        public string PartnerName { get; set; }
        public string PersonalContact { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Workphone { get; set; }
        public string Fax { get; set; }
        public string Taxcode { get; set; }
        public string MAWB { get; set; }
        public string HWBNO { get; set; }
        public decimal? Amount { get; set; }
        public string Curr { get; set; }
        public bool Dpt { get; set; }
        public decimal? ROBH { get; set; }
        public string CustomNo { get; set; }
        public string JobNo { get; set; }
        public string CdCode { get; set; }
        public string Docs { get; set; }
        public DateTime? DatetimeModifiedCdNote { get; set; }
    }

    public class CombineBillingReportParams
    {
        public string UptoDate { get; set; }
        public string dtPrintDate { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string IbanCode { get; set; }
        public string AccountName { get; set; }
        public string AccountNameEn { get; set; }
        public string BankName { get; set; }
        public string BankNameEn { get; set; }
        public string SwiftAccs { get; set; }
        public string AccsUSD { get; set; }
        public string AccsVND { get; set; }
        public string BankAddress { get; set; }
        public string BankAddressEn { get; set; }
        public string Paymentterms { get; set; }
        public string Contact { get; set; }
        public int CurrDecimalNo { get; set; }
        public string RefNo { get; set; }
        public string Email { get; set; }
    }
}
