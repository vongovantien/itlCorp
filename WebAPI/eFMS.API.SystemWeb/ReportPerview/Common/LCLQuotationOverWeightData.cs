using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportPerview.Common
{
    public class LCLQuotationOverWeightData
    {
        public decimal Ladder { get; set; }
        public decimal Price { get; set; }
        public string UnitName_VN { get; set; }
        public string UnitName_EN { get; set; }
        public string PickupName { get; set; }
        public string DeliveryName { get; set; }
        public string ConditionID { get; set; }       
    }
}