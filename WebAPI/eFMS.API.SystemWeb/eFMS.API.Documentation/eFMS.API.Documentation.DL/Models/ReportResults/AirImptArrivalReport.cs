using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class AirImptArrivalReport
    {
        public string HWBNO { get; set; }
        public string ArrivalNo { get; set; }
        public string Consignee { get; set; }
        public string ReferrenceNo { get; set; }
        public string FlightNo { get; set; }
        public string DepartureAirport { get; set; }
        public DateTime? CussignedDate { get; set; }
        public string LastDestination { get; set; }
        public string WarehouseDestination { get; set; }
        public string ShippingMarkImport { get; set; }
        public DateTime DatePackage { get; set; }
        public string NoPieces { get; set; }
        public string Description { get; set; }
        public decimal? WChargeable { get; set; }
        public bool blnShow { get; set; }
        public bool blnStick { get; set; }
        public bool blnRoot { get; set; }
        public string FreightCharge { get; set; }
        public decimal? Qty { get; set; }
        public string Unit { get; set; }
        public decimal? TotalValue { get; set; }
        public string Curr { get; set; }
        public decimal? VAT { get; set; }
        public string Notes { get; set; }
        public string ArrivalFooterNotice { get; set; }
        public string Shipper { get; set; }
        public decimal? CBM { get; set; }
        public string AOL { get; set; }
        public string KilosUnit { get; set; }
        public DateTime DOPickup { get; set; }
        public decimal ExVND { get; set; }
        public string AgentName { get; set; }
        public string Notify { get; set; }
        public string DecimalSymbol { get; set; }
        public string DigitSymbol { get; set; }
        public decimal DecimalNo { get; set; }
        public decimal CurrDecimalNo { get; set; }
    }

    public class AirImptArrivalReportParams
    {
        public string No { get; set; }
        public string MAWB { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string AccountInfo { get; set; }
        public string Contact { get; set; }
        public decimal DecimalNo { get; set; }
        public decimal CurrDecimalNo { get; set; }
    }
}
