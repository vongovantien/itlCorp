using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class AcctCdnoteModel : AcctCdnote
    {
        public TransactionTypeEnum TransactionTypeEnum { get; set; }
        public List<CsShipmentSurcharge> listShipmentSurcharge { get; set; }
        public int total_charge { get; set; }
        public string soaNo { get; set; }
        public object balanceCdNote { get; set; }
    }
}
