using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainMrrequestDetail
    {
        public Guid Id { get; set; }
        public Guid MrrequestId { get; set; }
        public int VehiclePartId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Tariff { get; set; }
        public decimal? Amount { get; set; }
        public int? LengthKm { get; set; }
        public string Remark { get; set; }
        public string RepairType { get; set; }
        public short? UnitId { get; set; }
        public string Serials { get; set; }
        public DateTime? LastReplacedDate { get; set; }
        public int? LastContermetNumber { get; set; }
        public decimal? LastReplacedQuantity { get; set; }
        public decimal? LastAmount { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
