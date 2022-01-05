using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class ExportSOAOPS
    {
        public string AOL { get; set; }
        public string CommodityName { get; set; }
        public string HwbNo { get; set; }
        public decimal? GW { get; set; }
        public decimal? CBM { get; set; }
        public string PackageContainer { get; set; }

        public List<ChargeSOAResult> Charges { get; set; }
    }
}
