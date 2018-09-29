using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsOrderDetailShortTrip
    {
        public Guid Id { get; set; }
        public Guid OrderDetailId { get; set; }
        public Guid PlaceFromId { get; set; }
        public Guid PlaceToId { get; set; }
        public string RoadId { get; set; }
        public int? SequentialNumber { get; set; }
        public Guid? RateCardConditionId { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
