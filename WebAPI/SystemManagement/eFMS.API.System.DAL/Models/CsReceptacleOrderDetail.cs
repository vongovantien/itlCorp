using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsReceptacleOrderDetail
    {
        public long Id { get; set; }
        public int ReceptacleMasterId { get; set; }
        public Guid OrderDetailId { get; set; }
        public Guid OrderItemId { get; set; }
        public int? SortCode { get; set; }
        public int? Quantity { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Volume { get; set; }
        public short? UnitId { get; set; }
        public string BarCode { get; set; }
        public string Remark { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
