using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsManifestEditModel: CsManifestModel
    {
        public List<CsTransactionDetailAddManifest> CsTransactionDetails { get; set; }
    }
}
