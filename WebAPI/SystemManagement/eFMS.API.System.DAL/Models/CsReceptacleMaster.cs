using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsReceptacleMaster
    {
        public CsReceptacleMaster()
        {
            CsReceptacleChecking = new HashSet<CsReceptacleChecking>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public Guid? OriginPlaceId { get; set; }
        public Guid? DestinationPlaceId { get; set; }
        public string SealNo { get; set; }
        public decimal? Weight { get; set; }
        public short? UnitId { get; set; }
        public int? Quantity { get; set; }
        public short ReceptacleTypeId { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Volume { get; set; }
        public decimal? VolumeFromMeasure { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public int? ReceptacleParentId { get; set; }
        public bool? Unbagged { get; set; }
        public string UnbaggedBy { get; set; }
        public DateTime? UnbaggedOn { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public CatReceptacleType ReceptacleType { get; set; }
        public ICollection<CsReceptacleChecking> CsReceptacleChecking { get; set; }
    }
}
