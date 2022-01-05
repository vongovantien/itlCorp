using eFMS.API.SystemFileManagement.DL.Models.SettlementPayment;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.Criteria
{
    public class ShipmentsCopyCriteria
    {
        public List<ShipmentChargeSettlement> charges { get; set; }
        public List<ShipmentsCopy> shipments { get; set; }
    }
}
