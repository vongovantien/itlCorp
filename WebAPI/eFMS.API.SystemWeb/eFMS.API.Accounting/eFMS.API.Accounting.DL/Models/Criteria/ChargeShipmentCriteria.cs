using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class ChargeShipmentCriteria
    {
        public string CurrencyLocal { get; set; }
        public string CustomerID { get; set; }
        public string DateType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Type { get; set; }
        public bool IsOBH { get; set; }
        public string StrCreators { get; set; }
        public string StrCharges { get; set; }
        public short? CommodityGroupID { get; set; }
        public string StrServices { get; set; }
        public List<string> JobIds { get; set; }
        public List<string> Hbls { get; set; }
        public List<string> Mbls { get; set; }
        public List<string> CustomNo { get; set; }

    }
}
