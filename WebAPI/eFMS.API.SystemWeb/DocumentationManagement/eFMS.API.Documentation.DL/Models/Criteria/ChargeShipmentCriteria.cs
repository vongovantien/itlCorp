using System;

namespace eFMS.API.Documentation.DL.Models.Criteria
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
    }
}
