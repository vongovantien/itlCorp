using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults.Sales
{
    public class CombinationSaleReportResult
    {
        public string Department { get; set; }
        public string ContactName { get; set; }
        public string PartnerName { get; set; }
        public string Description { get; set; }
        public string Area { get; set; }
        public string POL { get; set; }
        public string POD { get; set; }
        public string Lines { get; set; }
        public string Agent { get; set; }
        public string NominationParty { get; set; }
        public bool Assigned { get; set; }
        public string TransID { get; set; }
        public DateTime? LoadingDate { get; set; }
        public string HAWBNO { get; set; }
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
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string ShipmentSource { get; set; }

    }
}
