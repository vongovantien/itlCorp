using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsTransactionDetailAddManifest: CsTransactionDetail
    {
        public bool IsRemoved { get; set; }
    }
}
