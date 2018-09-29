using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsOrderDetailShipmentNote
    {
        public int Id { get; set; }
        public Guid OrderDetailId { get; set; }
        public int ShipmentNoteId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
    }
}
