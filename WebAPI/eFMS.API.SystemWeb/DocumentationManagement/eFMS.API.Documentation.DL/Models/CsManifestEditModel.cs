using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsManifestEditModel
    {
        public CsManifestModel CsManifest { get; set; }
        public List<CsTransactionDetail> CsTransactionDetails { get; set; }
    }
}
