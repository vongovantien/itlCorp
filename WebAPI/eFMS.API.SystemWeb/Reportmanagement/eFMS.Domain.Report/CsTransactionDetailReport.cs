using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.Domain.Report
{
    public class CsTransactionDetailReport
    {
        public string POL { get; set; }
        public string POD { get; set; }
        public string VesselNo { get; set; }
        public string ETD { get; set; }
        public string SealNoContainerNames { get; set; }
        public string NumberContainerTypes { get; set; }
        public List<HouseBillManifestModel> HouseBillManifestModels { get; set; }
    }
}
