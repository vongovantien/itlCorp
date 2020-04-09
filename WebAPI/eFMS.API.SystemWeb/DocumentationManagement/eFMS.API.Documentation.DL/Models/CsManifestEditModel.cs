using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsManifestEditModel: CsManifestModel
    {
        public List<CsTransactionDetailAddManifest> CsTransactionDetails { get; set; }
    }
}
