using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults.Sales
{
    public class MonthlySaleReportResult
    {
        public string Department { get; set; }
        public string ContactName { get; set; }
        public string SalesManager { get; set; }
        public string PartnerName { get; set; }
        public string Description { get; set; }
        public string Area { get; set; }
        public string POL { get; set; }
        public string POD { get; set; }	
        public string Lines { get; set; }
        public string Agent { get; set; }
        public string NominationParty { get; set; }
        public bool assigned { get; set; }
        public string TransID { get; set; }
        public DateTime? LoadingDate { get; set; }
        public string HWBNO { get; set; }
        public string Volumne { get; set; }
        public decimal Qty20 { get; set; }
        public decimal Qty40 { get; set; }
        public decimal Cont40HC { get; set; }
        public decimal KGS { get; set; }
        public decimal CBM { get; set; }
        public decimal SellingRate { get; set; }
        public decimal SharedProfit { get; set; }
        public decimal BuyingRate { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal SalesTarget { get; set; }
        public decimal Bonus { get; set; }
        public decimal DptSalesTarget { get; set; }
        public decimal DptBonus { get; set; }
        public string KeyContact { get; set; }
        public string MBLNO { get; set; }
        public string Vessel { get; set; }
        public string TypeOfService { get; set; }
        public string Shipper { get; set; }	
        public string Consignee { get; set; }
    }
}
