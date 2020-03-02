using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsAirWayBillModel: CsAirWayBill
    {
        public List<CsDimensionDetailModel> DimensionDetails { get; set; }
        public List<CsShipmentOtherChargeModel> OtherCharges { get; set; }
    }
}
