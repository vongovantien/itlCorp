using System;

namespace eFMS.API.Accounting.DL.Models.ReportResults
{
    public class AscSettlementPaymentRequestReport
    {
        public string AdvID { get; set; }
        public DateTime? AdvDate { get; set; }
        public string AdvContact { get; set; }
        public string AdvAddress { get; set; }
        public string StlDescription { get; set; }
        public string AdvanceNo { get; set; }
        public decimal? AdvValue { get; set; }
        public string AdvCurrency { get; set; }
        public decimal Remains { get; set; }
        public DateTime? AdvanceDate { get; set; }
        public decimal No { get; set; }
        public string CustomsID { get; set; }
        public string JobID { get; set; }
        public string HBL { get; set; }
        public string Description { get; set; }
        public string InvoiceNo { get; set; }
        public decimal Amount { get; set; }
        public bool Debt { get; set; }
        public string Currency { get; set; }
        public string Note { get; set; }
        public string AdvDpManagerID { get; set; }
        public bool AdvDpManagerStickDeny { get; set; }
        public bool AdvDpManagerStickApp { get; set; }
        public string AdvDpManagerName { get; set; }
        public DateTime? AdvDpSignDate { get; set; }
        public string AdvAcsDpManagerID { get; set; }
        public bool AdvAcsDpManagerStickDeny { get; set; }
        public bool AdvAcsDpManagerStickApp { get; set; }
        public string AdvAcsDpManagerName { get; set; }
        public DateTime? AdvAcsSignDate { get; set; }
        public string AdvBODID { get; set; }
        public bool AdvBODStickDeny { get; set; }
        public bool AdvBODStickApp { get; set; }
        public string AdvBODName { get; set; }
        public DateTime? AdvBODSignDate { get; set; }
        public string SltAcsCashierName { get; set; }
        public DateTime? SltCashierDate { get; set; }
        public bool Saved { get; set; }
        public bool ClearStatus { get; set; }
        public string Status { get; set; }
        public bool AcsApproval { get; set; }
        public string SltDpComment { get; set; }
        public string Shipper { get; set; }
        public string ShipmentInfo { get; set; }
        public string MBLNO { get; set; }
        public decimal VAT { get; set; }
        public decimal BFVATAmount { get; set; }
        public string ContainerQty { get; set; }
        public int Noofpieces { get; set; }
        public string UnitPieaces { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal NW { get; set; }
        public decimal CBM { get; set; }
        public string ShipperHBL { get; set; }
        public string ConsigneeHBL { get; set; }
        public string ModeSettle { get; set; }
        public int STT { get; set; }
        public string Series { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string Inword { get; set; }
        public string InvoiceID { get; set; }
        public string Commodity { get; set; }
        public string ServiceType { get; set; }
        public string SltCSName { get; set; }
        public bool SltCSStickDeny { get; set; }
        public bool SltCSStickApp { get; set; }
        public DateTime? SltCSSignDate { get; set; }
        public string SettlementNo { get; set; }
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
