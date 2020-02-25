using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportPerview.Common
{
    public class LCLQuotationDetailReportData
    {
        public decimal FromValue { get; set; }
        public decimal ToValue { get; set; }
        public decimal Price { get; set; }
        public string UnitName_VN { get; set; }
        public string UnitName_EN { get; set; }
        public bool IsMinCharges { get; set; }
        public string PickupName { get; set; }
        public string DeliveryName { get; set; }
        public string ConditionID { get; set; }
    }
}