using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SaleFclquotationShortTripDetail
    {
        public Guid QuotationShortTripId { get; set; }
        public Guid PlaceFrom { get; set; }
        public Guid PlaceTo { get; set; }
        public int LenthKm { get; set; }
        public bool? BelongToCustomer { get; set; }
        public int? Repetition { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
