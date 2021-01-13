namespace eFMS.API.Accounting.DL.Models.ExportResults
{
    public class InfoShipmentAdvanceExport
    {
        public string JobNo { get; set; }
        public string CustomNo { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string Customer { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string Container { get; set; }
        public string PersonInCharge { get; set; }
        public decimal? Cw { get; set; }
        public decimal? Pcs { get; set; }
        public decimal? Cbm { get; set; }
        public decimal? NormAmount { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public decimal? OtherAmount { get; set; }        
    }
}
