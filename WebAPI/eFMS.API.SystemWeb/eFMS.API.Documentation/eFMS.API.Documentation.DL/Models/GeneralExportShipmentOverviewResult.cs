using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class GeneralExportShipmentOverviewResult
    {
        public string ServiceName { get; set; }
        public string JobNo { get; set; }
        public DateTime? etd { get; set; }
        public DateTime? eta { get; set; }
        public string FlightNo { get; set; }
        public string MblMawb { get; set; }
        public string HblHawb { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public string PolPod { get; set; }
        public string Carrier { get; set; }
        public string Agent { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string ShipmentType { get; set; }
        public string Salesman { get; set; }
        public string AgentName { get; set; }
        public string QTy { get; set; }
        public short? PackageType { get; set; }
        public string Cont20 { get; set; }
        public string Cont40 { get; set; }
        public string Cont40HC { get; set; }
        public string Cont45 { get; set; }
        public decimal? GW { get; set; }
        public decimal? CW { get; set; }
        public decimal? CBM { get; set; }
        public decimal? TotalSellFreight { get; set; }
        public decimal? TotalSellTrucking { get; set; }
        public decimal? TotalSellHandling { get; set; }
        public decimal? TotalSellOthers { get; set; }
        public decimal? TotalSell { get; set; }

        public decimal? TotalBuyFreight { get; set; }
        public decimal? TotalBuyTrucking { get; set; }
        public decimal? TotalBuyHandling { get; set; }
        public decimal? TotalBuyOthers { get; set; }
        public decimal? TotalBuyKB { get; set; }

        public decimal? TotalBuy { get; set; }
        public decimal? Profit { get; set; }
        public decimal? AmountOBH { get; set; }
        public string CustomerId { get; set; }
        public string Destination { get; set; }
        public string CustomerName { get; set; }
        public string RalatedHblHawb { get; set; }
        public string RalatedJobNo { get; set; }
        public Guid? OfficeId { get; set; }
        public string HandleOffice { get; set; }
        public string SalesOffice { get; set; }
        public string Creator { get; set; }
        public string POINV { get; set; }
        public string BKRefNo { get; set; }
        public string Commodity { get; set; }
        public string ServiceMode { get; set; }
        public string PMTerm { get; set; }
        public string ShipmentNotes { get; set; }
        public DateTime? Created { get; set; }
        public Guid? HblId { get; set; }


    }
}
