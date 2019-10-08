using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class ShipmentsCopy
    {
        public int No { get; set; }
        public string JobId { get; set; }
        public string Customer { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public Guid HBLID { get; set; }
        public string CustomNo { get; set; }
        public string Service { get; set; }
    }
}
