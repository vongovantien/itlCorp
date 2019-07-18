using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class AcctCdnoteModel : AcctCdnote
    {
        public List<CsShipmentSurcharge> listShipmentSurcharge { get; set; }
    }
}
