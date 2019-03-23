using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class AcctSOAModel : AcctSoa
    {
        public List<CsShipmentSurcharge> listCharges { get; set; }
    }
}
