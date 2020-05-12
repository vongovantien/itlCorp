using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class CsMawbcontainer
    {
        public Guid Id { get; set; }
        public Guid? Mblid { get; set; }
        public Guid? Hblid { get; set; }
        public short? ContainerTypeId { get; set; }
        public int? Quantity { get; set; }
        public string ContainerNo { get; set; }
        public string SealNo { get; set; }
        public string MarkNo { get; set; }
        public short? UnitOfMeasureId { get; set; }
        public int? CommodityId { get; set; }
        public short? PackageTypeId { get; set; }
        public short? PackageQuantity { get; set; }
        public string Description { get; set; }
        public decimal? Gw { get; set; }
        public decimal? Nw { get; set; }
        public decimal? Cbm { get; set; }
        public decimal? ChargeAbleWeight { get; set; }
        public bool? Partof { get; set; }
        public string OwnerId { get; set; }
        public string OffHireDepot { get; set; }
        public string OffHireRefNo { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? IsPartOfContainer { get; set; }
    }
}
