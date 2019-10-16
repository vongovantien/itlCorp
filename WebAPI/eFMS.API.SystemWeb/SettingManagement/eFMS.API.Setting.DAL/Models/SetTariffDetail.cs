using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class SetTariffDetail
    {
        public Guid Id { get; set; }
        public Guid TariffId { get; set; }
        public Guid ChargeId { get; set; }
        public string UseFor { get; set; }
        public string Route { get; set; }
        public int? CommodityId { get; set; }
        public string PayerId { get; set; }
        public Guid? PortId { get; set; }
        public Guid? WarehouseId { get; set; }
        public string Type { get; set; }
        public string RangeType { get; set; }
        public string RangeFrom { get; set; }
        public string RangeTo { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public decimal? NextUnit { get; set; }
        public decimal? NextUnitPrice { get; set; }
        public short? UnitId { get; set; }
        public string CurrencyId { get; set; }
        public decimal? Vatrate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
