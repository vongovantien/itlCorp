using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatTransitRouteMiddlePlace
    {
        public int Id { get; set; }
        public Guid RouteId { get; set; }
        public Guid PlaceId { get; set; }
        public int Sequence { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
    }
}
