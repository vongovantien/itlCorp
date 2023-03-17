using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsWorkOrderSurchargeModel: CsWorkOrderSurcharge
    {
        public string PartnerName { get; set; }
        public string ChargeName { get; set; }
    }
}
