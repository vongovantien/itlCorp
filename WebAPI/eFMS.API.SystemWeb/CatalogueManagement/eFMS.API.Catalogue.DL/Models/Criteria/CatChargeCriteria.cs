using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatChargeCriteria
    {
        public string All { get; set; }
        public string Id { get; set; }
        public string ChargeNameVn { get; set; }
        public string ChargeNameEn { get; set; }
        public string ShipmentTypeId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
