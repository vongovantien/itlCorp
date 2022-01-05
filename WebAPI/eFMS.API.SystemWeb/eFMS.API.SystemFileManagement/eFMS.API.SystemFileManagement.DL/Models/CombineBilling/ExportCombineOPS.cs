using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class ExportCombineOPS
    {
        public string AOL { get; set; }
        public string CommodityName { get; set; }
        public string HwbNo { get; set; }
        public decimal? GW { get; set; }
        public decimal? CBM { get; set; }
        public string PackageContainer { get; set; }

        public List<ChargeCombineResult> Charges { get; set; }
    }
}
