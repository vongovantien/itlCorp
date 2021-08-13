using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class AirImptAuthorisedLetterReport
    {
        public string HWBNO { get; set; }
        public string DONo { get; set; }	
        public string Consignee { get; set; }
        public string ReferrenceNo { get; set; }
        public string FlightNo { get; set; }
        public DateTime? FlightDate { get; set; }
        public string DepartureAirport { get; set; }
        public DateTime CussignedDate { get; set; }
        public string LastDestination { get; set; }
        public string ShippingMarkImport { get; set; }
        public DateTime? DatePackage { get; set; }
        public string ReceivedCountry { get; set; }
        public string PlaceAtReceipt { get; set; }
        public string NoPieces { get; set; }
        public string Description { get; set; }
        public decimal? WChargeable { get; set; }
        public string DeliveryOrderNote { get; set; }
        public string FirstDestination { get; set; }
        public string SecondDestination { get; set; }
        public decimal? CBM { get; set; }
        public string KilosUnit { get; set; }
        public string AgentName { get; set; }
        public string Notify { get; set; }
        public string SignPath { get; set; }
    }
    public class AirImptAuthorisedLetterReportParameter
    {
        public string MAWB { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public int DecimalNo { get; set; }
        public string PrintDay { get; set; }
        public string PrintMonth { get; set; }
        public string PrintYear { get; set; }
    }
}
