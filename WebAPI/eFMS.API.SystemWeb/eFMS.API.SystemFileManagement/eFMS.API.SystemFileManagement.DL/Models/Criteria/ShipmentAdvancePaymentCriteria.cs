using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.Criteria
{
    public class ShipmentAdvancePaymentCriteria
    {
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string AdvanceNo { get; set; }
    }
}
