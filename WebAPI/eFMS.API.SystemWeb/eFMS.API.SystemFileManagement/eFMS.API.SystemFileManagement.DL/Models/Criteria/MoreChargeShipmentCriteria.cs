using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models.Criteria
{
    public class MoreChargeShipmentCriteria : ChargeShipmentCriteria
    {
        public List<ChargeShipmentModel> ChargeShipments { get; set; }

        public bool InSoa { get; set; }
        public string JobId { get; set; }
        public string Hbl { get; set; }
        public string Mbl { get; set; }
        public string CDNote { get; set; }
    }
}
