using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsFclbookingDetail
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime? ReceiptDatetime { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public DateTime? ReturnDatetime { get; set; }
        public int Quantity { get; set; }
        public int? Remain { get; set; }
        public string Description { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public decimal? Codvalue { get; set; }
    }
}
