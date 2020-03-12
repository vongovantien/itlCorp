namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class BookingNoteReport
    {
        public string FlexId { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string HawbNo { get; set; }
        public string MawbNo { get; set; }
        public string FlightNo1 { get; set; }
        public string FlightNo2 { get; set; }
        public string DepartureAirport { get; set; }
        public string PlaceOfReceipt { get; set; }
        public string AirportOfDischarge { get; set; }
        public string DestinationAirport { get; set; }
        public string TotalCollect { get; set; }
        public string TotalPrepaid { get; set; }
        public string ShippingMark { get; set; }
        public decimal? Pieces { get; set; }
        public string DesOfGood { get; set; }
        public decimal? Gw { get; set; }
        public decimal? Cbm { get; set; }
        public string ContactPerson { get; set; }
        public string ClosingTime { get; set; }
        public string CurrentUser { get; set; }
        public string CurrentDate { get; set; }
    }
}
