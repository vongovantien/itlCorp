using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class GroupChargeModel
    {
        public string Hwbno { get; set; }
        public string Hbltype { get; set; }
        public Guid Id { get; set; }
        public List<CsShipmentSurchargeDetailsModel> listCharges { get; set; }
        public string FlexId { get; set; }
    }
}
