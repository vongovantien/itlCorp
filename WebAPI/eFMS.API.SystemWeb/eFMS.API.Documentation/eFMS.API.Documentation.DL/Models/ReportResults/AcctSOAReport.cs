using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class AcctSOAReport
    {
        public int? SortIndex { get; set; }
        public string Subject { get; set; }
        public string PartnerID { get; set; }
        public string PartnerName { get; set; }
        public string PersonalContact { get; set; }
        public string Address { get; set; }
        public string Taxcode { get; set; }
        public string Workphone { get; set; }
        public string Fax { get; set; }
        public string TransID { get; set; }
        public DateTime? TransDate { get; set; }
        public DateTime? LoadingDate { get; set; }
        public string Commodity { get; set; }
        public string PortofLading { get; set; }
        public string PortofUnlading { get; set; }
        public string MAWB { get; set; }
        public string Invoice { get; set; }
        public string EstimatedVessel { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public decimal? Noofpieces { get; set; }
        public string UnitPieaces { get; set; }
        public DateTime? Delivery { get; set; }
        public string HWBNO { get; set; }
        public string Description { get; set; }
        public decimal? Quantity { get; set; }
        public string QUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public string Unit { get; set; }
        public decimal? VAT { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string Notes { get; set; }
        public string InputData { get; set; }
        public string PONo { get; set; }
        public string TransNotes { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string ContQty { get; set; }
        public string ContSealNo { get; set; }
        public decimal? Deposit { get; set; }
        public string DepositCurr { get; set; }
        public string DecimalSymbol { get; set; }
        public string DigitSymbol { get; set; }
        public decimal? DecimalNo { get; set; }
        public decimal? CurrDecimalNo { get; set; }
        public string VATInvoiceNo { get; set; }
        public decimal? GW { get; set; }
        public decimal? NW { get; set; }
        public decimal? SeaCBM { get; set; }
        public string SOTK { get; set; }
        public string Cuakhau { get; set; }
        public string DeliveryPlace { get; set; }
        public DateTime? NgayDK { get; set; }
        public DateTime? CustomDate { get; set; }
        public string JobNo { get; set; }
        public decimal? ExchangeRateToUsd { get; set; }
        public decimal? ExchangeRateToVnd { get; set; }
        public decimal? ExchangeVATToUsd { get; set; }
    }


    public class AcctSOAReportParams
    {
        public string DBTitle { get; set; }
        public string DebitNo { get; set; }
        public string TotalDebit { get; set; }
        public string TotalCredit { get; set; }
        public string DueToTitle { get; set; }
        public string DueTo { get; set; }
        public string DueToCredit { get; set; }
        public string SayWordAll { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string IbanCode { get; set; }
        public string AccountName { get; set; }
        public string AccountNameEN { get; set; }
        public string BankName { get; set; }
        public string BankNameEN { get; set; }
        public string SwiftAccs { get; set; }
        public string AccsUSD { get; set; }
        public string AccsVND { get; set; }
        public string BankAddress { get; set; }
        public string BankAddressEN { get; set; }
        public string Paymentterms { get; set; }
        public decimal? DecimalNo { get; set; }
        public decimal? CurrDecimal { get; set; }
        public string IssueInv { get; set; }
        public string InvoiceInfo { get; set; }
        public string Contact { get; set; }
        public string IssuedDate { get; set; }
        public string OtherRef { get; set; }
        public bool IsOrigin { get; set; }

    }
}
