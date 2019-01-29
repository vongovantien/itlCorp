using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class TestContainerList
    {
        public Guid Id { get; set; }
        public string ContainerType { get; set; }
        public int ContainerQuantity { get; set; }
        public string ContainerNo { get; set; }
        public string SealNo { get; set; }
        public string MarkNo { get; set; }
        public int? CommodityId { get; set; }
        public string PackageTypeId { get; set; }
        public string GoodsDescription { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? ChargeAbleWeight { get; set; }
        public short? UnitId { get; set; }
        public decimal? Cbm { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string JobId { get; set; }
        public string Hblno { get; set; }
    }
}
