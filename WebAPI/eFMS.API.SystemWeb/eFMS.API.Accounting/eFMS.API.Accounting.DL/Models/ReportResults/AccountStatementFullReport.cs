using System;

namespace eFMS.API.Accounting.DL.Models.ReportResults
{
    public class AccountStatementFullReport
    {
        public string PartnerID { get; set; }
        public string PartnerName { get; set; }
        public string PersonalContact { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Workphone { get; set; }
        public string Fax { get; set; }
        public string Taxcode { get; set; }
        public string TransID { get; set; }
        public string MAWB { get; set; }
        public string HWBNO { get; set; }
        public string DateofInv { get; set; }
        public string Order { get; set; }
        public string InvID { get; set; }
        public decimal? Amount { get; set; }
        public string Curr { get; set; }
        public bool Dpt { get; set; }
        public string Vessel { get; set; }
        public string Routine { get; set; }
        public DateTime? LoadingDate { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public string TpyeofService { get; set; }
        public string SOANO { get; set; }
        public DateTime? SOADate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? OAmount { get; set; }
        public decimal? SAmount { get; set; }
        public string CurrOP { get; set; }
        public string Notes { get; set; }
        public string IssuedBy { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string OtherRef { get; set; }
        public string Volumne { get; set; }
        public decimal? POBH { get; set; }
        public decimal? ROBH { get; set; }
        public string CustomNo { get; set; }
        public string JobNo { get; set; }
        public string CdCode { get; set; }
        public string Docs { get; set; }
    }

    public class AccountStatementFullReportParams
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
