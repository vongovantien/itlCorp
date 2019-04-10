using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class ManifestReportResult
    {
        public string POL { get; set; }
        public string POD { get; set; }
        public string VesselNo { get; set; }
        public DateTime? ETD { get; set; }
        public string SealNoContainerNames { get; set; }
        public string NumberContainerTypes { get; set; }
    }
    public class HouseBillManifestModel
    {
        public string Hwbno { get; set; }
        public string Packages { get; set; }
        public decimal Weight { get; set; }
        public decimal Volumn { get; set; }
        public string Shipper { get; set; }
        public string NotifyParty { get; set; }
        public string ShippingMark { get; set; }
        public string Description { get; set; }
        public string FreightPayment { get; set; }
        public decimal TotalPackage { get; set; }
        public decimal SumGrossWeight { get; set; }
        public decimal SumVolumn { get; set; }
    }
}
