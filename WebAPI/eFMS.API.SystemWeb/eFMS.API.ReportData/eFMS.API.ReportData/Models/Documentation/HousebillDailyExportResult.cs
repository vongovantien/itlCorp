namespace eFMS.API.ReportData.Models.Documentation
{
    public class HousebillDailyExportResult
    {
        public string Mawb { get; set; }
        public string Hawb { get; set; }
        public string FlightNo { get; set; }
        public string PodCode { get; set; }
        public string ShipperName { get; set; }
        public int? Pieces { get; set; } //Package Qty
        public string Po { get; set; }
        public string Remark { get; set; }
        public string WarehouseName { get; set; }
        public string PicName { get; set; }
    }
}
