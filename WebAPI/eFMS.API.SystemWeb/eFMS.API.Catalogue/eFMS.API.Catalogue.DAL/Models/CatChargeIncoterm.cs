using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatChargeIncoterm
    {
        public Guid Id { get; set; }
        public Guid IncotermId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public Guid? ChargeId { get; set; }
        public string QuantityType { get; set; }
        public int? Unit { get; set; }
        public int? ChargeTo { get; set; }
        public string Currency { get; set; }
        public string FeeType { get; set; }
        public string Type { get; set; }
    }
}
