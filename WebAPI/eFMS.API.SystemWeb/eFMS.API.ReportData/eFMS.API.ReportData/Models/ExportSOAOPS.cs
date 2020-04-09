using System.Collections.Generic;

namespace eFMS.API.ReportData.Models
{
    public class ExportSOAOPS
    {
        public string CommodityName { get; set; }
        public string HwbNo { get; set; }
        public decimal? GW { get; set; }
        public decimal? CBM { get; set; }
        public string PackageContainer { get; set; }
        public string AOL { get; set; }
        public List<SoaOpsChargeModel> Charges { get; set; }
    }
}
