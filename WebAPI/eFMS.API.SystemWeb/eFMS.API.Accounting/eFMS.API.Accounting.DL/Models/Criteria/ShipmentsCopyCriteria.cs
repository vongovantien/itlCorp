using eFMS.API.Accounting.DL.Models.SettlementPayment;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class ShipmentsCopyCriteria
    {
        public List<ShipmentChargeSettlement> charges { get; set; }
        public List<ShipmentsCopy> shipments { get; set; }
    }
}
