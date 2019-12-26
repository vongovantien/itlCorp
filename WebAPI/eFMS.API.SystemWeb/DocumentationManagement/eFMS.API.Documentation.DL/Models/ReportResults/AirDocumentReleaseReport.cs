using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class AirDocumentReleaseReport
    {
        public string HWBNO { get; set; }
        public string DONo { get; set; }
        public string Consignee { get; set; }
        public string ReferrenceNo { get; set; }
        public string FlightNo { get; set; }
        public DateTime FlightDate { get; set; }
        public string DepartureAirport { get; set; }
        public DateTime CussignedDate { get; set; }
        public string LastDestination { get; set; }
        public string ShippingMarkImport { get; set; }
        public DateTime DatePackage { get; set; }
        public string ReceivedCountry { get; set; }
        public string PlaceAtReceipt { get; set; }
        public string NoPieces { get; set; }
        public string Description { get; set; }
        public decimal WChargeable { get; set; }
        public string DeliveryOrderNote { get; set; }
        public string FirstDestination { get; set; }
        public string SecondDestination { get; set; }
        public decimal CBM { get; set; }
    }
    public class AirDocumentReleaseReportParams
    {
        public string MAWB { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string Contact { get; set; }
        public decimal DecimalNo { get; set; }
        public decimal JobID { get; set; }
    }
}
