using System;

namespace eFMS.API.Accounting.DL.Models.ReportResults
{
    public class AscSettlementPaymentRequestReport
    {
        public string AdvID { get; set; }
        public string Description { get; set; }
        public string InvoiceNo { get; set; }
        public decimal Amount { get; set; }
        public bool Debt { get; set; }
        public string Currency { get; set; }
        public string Note { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string Website { get; set; }
        public string Tel { get; set; }
        public string Contact { get; set; }
        public string Inword { get; set; }
        public string JobId { get; set; }
        public string JobIdFirst { get; set; }
        public string SettleRequester { get; set; }
        public string SettleRequestDate { get; set; }
        public string AdvValue { get; set; }
        public string AdvCurrency { get; set; }
        public string AdvDate { get; set; }
        public string SettlementNo { get; set; }
        public string Customer { get; set; }
        public string Consignee { get; set; }
        public string Consigner { get; set; }
        public string ContainerQty { get; set; }
        public decimal? GW { get; set; }
        public decimal? NW { get; set; }
        public string CustomsId { get; set; }
        public int? PSC { get; set; }
        public decimal? CBM { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string StlCSName { get; set; }
        public string StlDpManagerName { get; set; }
        public string StlDpManagerSignDate { get; set; }
        public string StlAscDpManagerName { get; set; }
        public string StlAscDpManagerSignDate { get; set; }
        public string StlBODSignDate { get; set; }
    }

    public class AscSettlementPaymentRequestReportParams
    {
        public string CompanyName { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string Contact { get; set; }
        public string Inword { get; set; }
        public string JobId { get; set; }
        public string SettleRequester { get; set; }
        public string SettleRequestDate { get; set; }
        public string AdvValue { get; set; }
        public string AdvCurrency { get; set; }
        public string AdvDate { get; set; }
        public string SettlementNo { get; set; }
        public string Customer { get; set; }
        public string Consignee { get; set; }
        public string Consigner { get; set; }
        public string ContainerQty { get; set; }
        public decimal? GW { get; set; }
        public decimal? NW { get; set; }
        public string CustomsId { get; set; }
        public int? PSC { get; set; }
        public decimal? CBM { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string StlCSName { get; set; }
        public string StlDpManagerName { get; set; }
        public string StlDpManagerSignDate { get; set; }
        public string StlAscDpManagerName { get; set; }
        public string StlAscDpManagerSignDate { get; set; }
        public string StlBODSignDate { get; set; }
    }
}
