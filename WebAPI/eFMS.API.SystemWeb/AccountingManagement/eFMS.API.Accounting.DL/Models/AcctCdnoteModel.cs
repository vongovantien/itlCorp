using eFMS.API.Accounting.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctCdnoteModel : AcctCdnote
    {
        public List<CsShipmentSurcharge> listShipmentSurcharge { get; set; }
    }
}
