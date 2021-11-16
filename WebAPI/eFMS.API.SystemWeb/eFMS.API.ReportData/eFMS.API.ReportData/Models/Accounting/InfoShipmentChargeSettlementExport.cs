namespace eFMS.API.ReportData.Models.Accounting
{
    public class InfoShipmentChargeSettlementExport
    {
        public string ChargeName { get; set; }
        public decimal? ChargeNetAmount { get; set; }
        public decimal? ChargeVatAmount { get; set; }
        public decimal? ChargeAmount { get; set; }
        public string InvoiceNo { get; set; }
        public string ChargeNote { get; set; }
        public string ChargeType { get; set; }
        public string SurType { get; set; }
        public decimal? ChargeAmountVND { get; set; }
    }
}
