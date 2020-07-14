using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class ShimentConverPageSeaReport
    {
        public string TransID { get; set; }
        public DateTime? TransDate { get; set; }
        public string HWBNO { get; set; }
        public string MAWB { get; set; }
        public string PartnerName { get; set; }
        public string ContactName { get; set; }
        public string ShipmentType { get; set; }
        public string NominationParty { get; set; }
        public bool? Nominated { get; set; }
        public string POL { get; set; }
        public string POD { get; set; }
        public string Commodity { get; set; }
        public string Volumne { get; set; }
        public string Carrier { get; set; }
        public string Agent { get; set; }
        public string ATTN { get; set; }
        public string Consignee { get; set; }
        public string ContainerNo { get; set; }
        public string OceanVessel { get; set; }
        public string LocalVessel { get; set; }
        public string FlightNo { get; set; }
        public string SeaImpVoy { get; set; }
        public string LoadingDate { get; set; }
        public string ArrivalDate { get; set; }
        public string Notes { get; set; }
        public string Finished { get; set; }
        public string SalesManager { get; set; }
        public string Prepairedby { get; set; }
        public string DocsManager { get; set; }
        public string PlaceDelivery { get; set; }
        public string DestETA { get; set; }
        public DateTime? ETA { get; set; }
        public DateTime? ETD { get; set; }

        public string DocsReleaseDate { get; set; }
        public string DetailNotes { get; set; }
        public string BKGNo { get; set; }
        public string NoPieces { get; set; }
        public string FreightTerm { get; set; }
        public decimal? GW { get; set; }
        public decimal? CW { get; set; }
        public string UnitW { get; set; }
        public decimal? CBM { get; set; }
        public string ShipmentSource { get; set; }
        public string Vessel { get; set; }

    }
    public class ShimentConverPageSeaReportParams
    {
        public string Contact { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public decimal DecimalNo { get; set; }
        public decimal CurrDecimalNo { get; set; }
        public string HBLList { get; set; }
    }
}
