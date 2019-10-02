using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.SettlementPayment
{
    public class ShipmentsCopy
    {
        public int No { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string CustomNo { get; set; }
        public string Service { get; set; }
    }
}
