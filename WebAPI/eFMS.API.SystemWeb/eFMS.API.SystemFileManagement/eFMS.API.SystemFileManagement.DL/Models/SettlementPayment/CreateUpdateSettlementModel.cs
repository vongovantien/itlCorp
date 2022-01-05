using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.SettlementPayment
{
    public class CreateUpdateSettlementModel
    {
        public AcctSettlementPaymentModel Settlement { get; set; }
        public List<ShipmentChargeSettlement> ShipmentCharge { get; set; }
    }
}
