using System;

namespace eFMS.API.Report.DL.Models
{
    public class SaleKickBackReportResult
    {
        public string TransID { get; set; }
        public Guid HBLID { get; set; }
        public DateTime? LoadingDate { get; set; }
        public string HAWBNO { get; set; }
        public string PartnerName { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public decimal UsdExt { get; set; }
        public string PaymentNo { get; set; }
        public string MAWB { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string PartnerID { get; set; }
        public string PartnerName2 { get; set; }
        public string PartnerName3 { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string TEL { get; set; }
        public string Cell { get; set; }
        public string Taxcode { get; set; }
        public string Category { get; set; }
        public string BankAccsNo { get; set; }
        public string BankName { get; set; }
        public string BankAddress { get; set; }
        public string Customer { get; set; }
        public string Coloader { get; set; }
        public string Agent { get; set; }
        public string CustomerINVUnpaid { get; set; }
        public string CustomerINVpaid { get; set; }
        public string AgentINVUnpaid { get; set; }
        public string AgentINVpaid { get; set; }
        public string CarrierINVUnpaid { get; set; }
        public string CarrierINVpaid { get; set; }
        public decimal Costs { get; set; }
        public decimal Incomes { get; set; }
    }
}
