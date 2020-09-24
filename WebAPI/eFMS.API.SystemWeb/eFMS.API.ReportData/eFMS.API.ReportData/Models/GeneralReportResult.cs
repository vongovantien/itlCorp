using System;

namespace eFMS.API.ReportData.Models
{
    public class GeneralReportResult
    {
        public int? No { get; set; }
        public string JobId { get; set; }
        public string Mawb { get; set; }
        public Guid? HblId { get; set; }
        public string Hawb { get; set; }
        public string CustomerName { get; set; }
        public string CustomerId { get; set; }
        public string CarrierName { get; set; }
        public string CarrierId { get; set; }
        public string AgentName { get; set; }
        public string AgentId { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string Route { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public int? Qty { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Profit { get; set; }
        public decimal? Obh { get; set; }
        public string PersonInCharge { get; set; }
        public string PicId { get; set; }
        public string Salesman { get; set; }
        public string SalesmanId { get; set; }
        public string Service { get; set; }
        public string ServiceName { get; set; }
        public decimal? ChargeWeight { get; set; }
    }
}
