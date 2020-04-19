using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class ManifestReportModel: CsManifestModel
    {
        public string SealNoContainerNames { get; set; }
        public string NumberContainerTypes { get; set; }

        public List<CsTransactionDetailModel> CsTransactionDetails { get; set; }
    }
}
