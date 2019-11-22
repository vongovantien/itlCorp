using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class ManifestReportModel: CsManifestModel
    {
        public string SealNoContainerNames { get; set; }
        public string NumberContainerTypes { get; set; }

        public List<CsTransactionDetailModel> CsTransactionDetails { get; set; }
        //public List<CsMawbcontainerModel> CsMawbcontainers { get; set; }
    }
}
