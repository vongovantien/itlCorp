using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsShipmentOtherCharge
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid? Hblid { get; set; }
        public string ChargeName { get; set; }
        public decimal? Amount { get; set; }
        public string DueTo { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
