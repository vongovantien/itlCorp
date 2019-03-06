using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class vw_csMAWBContainer
    {
        public Guid ID { get; set; }
        public Guid MBLID { get; set; }
        public Nullable<Guid> HBLID { get; set; }
        public Nullable<short> ContainerTypeID { get; set; }
        public string ContainerTypeName { get; set; }
        public Nullable<int> Quantity { get; set; }
        public string ContainerNo { get; set; }
        public string SealNo { get; set; }
        public string MarkNo { get; set; }
        public Nullable<short> UnitOfMeasureID { get; set; }
        public string UnitOfMeasureName { get; set; }
        public Nullable<int> CommodityId { get; set; }
        public string CommodityName { get; set; }
        public Nullable<short> PackageTypeId { get; set; }
        public string PackageTypeName { get; set; }
        public Nullable<short> PackageQuantity { get; set; }
        public string Description { get; set; }
        public Nullable<decimal> GW { get; set; }
        public Nullable<decimal> NW { get; set; }
        public Nullable<decimal> CBM { get; set; }
        public Nullable<decimal> ChargeAbleWeight { get; set; }
        public Nullable<bool> Partof { get; set; }
        public string OwnerID { get; set; }
        public string OffHireDepot { get; set; }
        public string OffHireRefNo { get; set; }
        public string UserModified { get; set; }
        public Nullable<DateTime> DatetimeModified { get; set; }
    }
}
