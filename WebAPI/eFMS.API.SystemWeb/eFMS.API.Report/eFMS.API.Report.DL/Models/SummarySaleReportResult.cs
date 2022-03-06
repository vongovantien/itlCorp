
namespace eFMS.API.Report.DL.Models
{
    public class SummarySaleReportResult
    {
        public string Department { get; set; }
        public string ContactName { get; set; }
        public string PartnerName { get; set; }
        public string Description { get; set; }
        public bool Assigned { get; set; }
        public string TransID { get; set; }
        public decimal? Qty20 { get; set; }
        public decimal? Qty40 { get; set; }
        public decimal? Cont40HC { get; set; }
        public decimal? KGS { get; set; }
        public decimal? CBM { get; set; }
        public decimal? SellingRate { get; set; }
        public decimal? SharedProfit { get; set; }
        public decimal? BuyingRate { get; set; }
        public decimal? OtherCharges { get; set; }
        public string TpyeofService { get; set; }
    }
}
