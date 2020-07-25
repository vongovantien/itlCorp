using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class GroupShipmentModel
    {
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string TotalDebit { get; set; }
        public string TotalCredit { get; set; }
        public List<ChargeShipmentModel> ChargeShipments { get; set; }
    }
}
