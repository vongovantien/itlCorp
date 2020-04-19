using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class AirShipperDebitNewReport
    {
        public decimal IndexSort { get; set; }
        public string Subject { get; set; }
        public string PartnerID { get; set; }
        public string PartnerName { get; set; }
        public string PersonalContact { get; set; }
        public string Address { get; set; }
        public string Workphone { get; set; }
        public string Fax { get; set; }
        public string Cell { get; set; }
        public string TaxCode { get; set; }
        public string TransID { get; set; }
        public DateTime? LoadingDate { get; set; }
        public string MAWB { get; set; }
        public string HWBNO { get; set; }
        public string FlightNo { get; set; }
        public DateTime? FlightDate { get; set; }
        public string DepartureAirport { get; set; }
        public string LastDestination { get; set; }
        public string Consignee { get; set; }
        public string ATTN { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? WChargeable { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public string QUnit { get; set; }
        public string Unit { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? VAT { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal ExtVND { get; set; }
        public string Notes { get; set; }
        public string InputData { get; set; }
        public decimal CTNSQty { get; set; }
        public string CTNSUnit { get; set; }
        public decimal Deposit { get; set; }
        public string DepositCurr { get; set; }
        public string Commodity { get; set; }
        public string DecimalSymbol { get; set; }
        public string DigitSymbol { get; set; }
        public decimal DecimalNo { get; set; }
        public decimal CurrDecimalNo { get; set; }
        public string FlexID { get; set; }
    }

    public class AirShipperDebitNewReportParams
    {
        public string DBTitle { get; set; }
        public string DebitNo { get; set; }
        public string TotalDebit { get; set; }
        public string TotalCredit { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string AccountName { get; set; }
        public string AccountNameEN { get; set; }
        public string BankName { get; set; }
        public string BankNameEN { get; set; }
        public string SwiftAccs { get; set; }
        public string AccsUSD { get; set; }
        public string AccsVND { get; set; }
        public string BankAddress { get; set; }
        public string BankAddressEN { get; set; }
        public decimal DecimalNo { get; set; }
        public string IssueInv { get; set; }
        public string InvoiceInfo { get; set; }
        public string Contact { get; set; }
        public string ReviseNotice { get; set; }
        public string IssuedDate { get; set; }
        public string HBLList { get; set; }
        public string OtherRef { get; set; }
        public string InwordVND { get; set; }
        public string Currency { get; set; }              
        public decimal RateUSDToVND { get; set; }
        public string BalanceAmount { get; set; }
    }
}
