using System;

namespace eFMS.API.Accounting.DL.Models.ReportResults
{
    public class AdvancePaymentRequestReport
    {
        public string AdvID { get; set; }
        public string RefNo { get; set; }
        public DateTime? AdvDate { get; set; }
        public string AdvTo { get; set; }
        public string AdvContactID { get; set; }
        public string AdvContact { get; set; }
        public string AdvAddress { get; set; }
        public decimal? AdvValue { get; set; }
        public string AdvCurrency { get; set; }
        public string AdvCondition { get; set; }
        public string AdvRef { get; set; }
        public string AdvHBL { get; set; }
        public DateTime? AdvPaymentDate { get; set; }
        public string AdvPaymentNote { get; set; }
        public string AdvDpManagerID { get; set; }
        public bool? AdvDpManagerStickDeny { get; set; }
        public bool? AdvDpManagerStickApp { get; set; }
        public string AdvDpManagerName { get; set; }
        public DateTime? AdvDpSignDate { get; set; }
        public string AdvAcsDpManagerID {get;set;}
        public bool? AdvAcsDpManagerStickDeny { get; set; }
        public bool? AdvAcsDpManagerStickApp { get; set; }
        public string AdvAcsDpManagerName { get; set; }
        public DateTime? AdvAcsSignDate { get; set; }
        public string AdvBODID { get; set; }
        public bool? AdvBODStickDeny { get; set; }
        public bool? AdvBODStickApp { get; set; }
        public string AdvBODName { get; set; }
        public DateTime? AdvBODSignDate { get; set; }
        public string AdvCashier { get; set; }
        public string AdvCashierName { get; set; }
        public DateTime? CashedDate { get; set; }
        public bool? Saved { get; set; }
        public string SettleNo { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal? AmountSettle { get; set; }
        public string SettleCurrency { get; set; }
        public bool? ClearStatus { get; set; }
        public string Status { get; set; }
        public bool? AcsApproval { get; set; }
        public string Description { get; set; }
        public string JobNo { get; set; }
        public string MAWB { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string InvoiceNo { get; set; }
        public string CustomID { get; set; }
        public string HBLNO { get; set; }
        public bool? Norm { get; set; }
        public bool? Validfee { get; set; }
        public bool? Others { get; set; }
        public bool? CSApp { get; set; }
        public bool? CSDecline { get; set; }
        public string CSUser { get; set; }
        public DateTime? CSAppDate { get; set; }
        public string Customer { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string ContQty { get; set; }
        public decimal? Noofpieces { get; set; }
        public string UnitPieaces { get; set; }
        public decimal? GW { get; set; }
        public decimal? NW { get; set; }
        public decimal? CBM { get; set; }
        public string ServiceType { get; set; }
        public string AdvCSName { get; set; }
        public DateTime? AdvCSSignDate { get; set; }
        public bool? AdvCSStickApp { get; set; }
        public bool? AdvCSStickDeny { get; set; }
        public decimal? TotalNorm { get; set; }
        public decimal? TotalInvoice { get; set; }
        public decimal? TotalOrther { get; set; }
        public string Inword { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string Website { get; set; }
        public string Contact { get; set; }
    }
}
