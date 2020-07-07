using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class SeaDeliveryCommandResult
    {
        public string DONo { get; set; }
        public DateTime DateConfirm { get; set; }
        public DateTime? FlightDate { get; set; }
        public string  LocalVessel { get; set; }	
        public string ContSealNo { get; set; }
        public string ForCarrier { get; set; }
        public string ATTN { get; set; }
        public string Consignee { get; set; }
        public string Notify { get; set; }
        public string HandlingInfo { get; set; }
        public string ExecutedOn { get; set; }
        public string OceanVessel { get; set; }	
        public string DepartureAirport { get; set; }
        public string PortofDischarge { get; set; }
        public string PlaceDelivery { get; set; }
        public string HWBNO { get; set; }
        public string ShippingMarkImport { get; set; }
        public string SpecialNote { get; set; }
        public string Description { get; set; }
        public string TotalPackages { get; set; }
        public string NoPieces { get; set; }
        public decimal? GrossWeight { get; set; }
        public string Unit { get; set; }	
        public decimal? CBM { get; set; }
        public bool SeaLCL { get; set; }
        public DateTime DatePackage { get; set; }
        public string DeliveryOrderNote { get; set; }
        public string FirstDestination { get; set; }
        public string SecondDestination { get; set; }
        public string BillType { get; set; }
        public string ArrivalNote { get; set; }
        public string FinalDestination { get; set; }

    }

    public class SeaDeliveryCommandParam
    {
        public string Consignee { get; set; }
        public string No { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string MAWB { get; set; }
        public string Contact { get; set; }
        public int DecimalNo { get; set; }
    }
}
