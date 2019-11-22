using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class ManifestFCLImportReport
    {
        public string TransID { get; set; }	
        public DateTime? LoadingDate { get; set; }
        public string LocalVessel { get; set; }
        public string ContSealNo { get; set; }
        public DateTime? DateConfirm { get; set; }
        public DateTime? FlightDate { get; set; }
        public string DepartureAirport { get; set; }
        public string PortofDischarge { get; set; }
        public string PlaceDelivery { get; set; }
        public string ForCarrier { get; set; }
        public string HWBNO { get; set; }
        public string ATTN { get; set; }
        public string Consignee { get; set; }
        public string Notify { get; set; }
        public string TotalPackages { get; set; }
        public string ShippingMarkImport { get; set; }
        public string SpeacialNote { get; set; }
        public bool SeaLCL { get; set; }
        public string Description { get; set; }
        public string NoPieces { get; set; }
        public decimal? GrossWeight { get; set; }
        public string Unit { get; set; }
        public decimal? CBM { get; set; }
        public DateTime? HBLDate { get; set; }
        public string AlsoNotify { get; set; }
        public DateTime? ManifestDate { get; set; }
        public DateTime? ETD { get; set; }
        public string TotalQty { get; set; }
        public string Liner { get; set; }
        public string TransMode { get; set; }
        public string SCName { get; set; }
        public string OverseasAgent { get; set; }
    }
}
