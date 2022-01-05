using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.Criteria
{
    public class AddMoreChargeCriteria
    {
        public List<ChargeShipmentModel> ChargeShipmentsCurrent { get; set; }
        public List<ChargeShipmentModel> ChargeShipmentsAddMore { get; set; }
    }
}
