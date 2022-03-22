using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class GroupShipmentModel
    {
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string TotalDebit { get; set; }
        public string TotalCredit { get; set; }
        public string ShipmentId { get; set; }
        public string PIC { get; set; }
        public List<ChargeShipmentModel> ChargeShipments { get; set; }
    }
}
