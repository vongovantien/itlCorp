using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsDtborder
    {
        public CsDtborder()
        {
            CsDtborderDropPointItemRoute = new HashSet<CsDtborderDropPointItemRoute>();
        }

        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Awb { get; set; }
        public string CustomerId { get; set; }
        public string SalePersonId { get; set; }
        public string Contact { get; set; }
        public DateTime ReceivedBookingDate { get; set; }
        public Guid OriginBranchId { get; set; }
        public Guid? OriginHubId { get; set; }
        public Guid? DestinationBranchId { get; set; }
        public Guid? DestinationHubId { get; set; }
        public DateTime PickupRequestDate { get; set; }
        public short VehicleTypeId { get; set; }
        public Guid PlaceFromId { get; set; }
        public Guid? DistrictFromId { get; set; }
        public Guid? ProvinceFromId { get; set; }
        public short? CountryFromId { get; set; }
        public Guid PlaceToId { get; set; }
        public Guid? DistrictToId { get; set; }
        public Guid? ProvinceToId { get; set; }
        public short? CountryToId { get; set; }
        public int? CommodityId { get; set; }
        public int? CurrentStatusId { get; set; }
        public Guid? RateCardDetailId { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalExcludeSurcharge { get; set; }
        public decimal? TotalSurcharge { get; set; }
        public decimal? TotalExcludeVat { get; set; }
        public bool? TaxExemption { get; set; }
        public bool? MapedDropPoint { get; set; }
        public int? Vatrate { get; set; }
        public decimal? Vat { get; set; }
        public decimal? TotalIncludeVat { get; set; }
        public decimal? AdjustedPrice { get; set; }
        public string CurrencyId { get; set; }
        public decimal? TotalCost { get; set; }
        public string CustomerDebitId { get; set; }
        public string CustomerBookingNo { get; set; }
        public string WarehouseBookingNo { get; set; }
        public bool? IsNotInFixedProject { get; set; }
        public string Remark { get; set; }
        public bool? Soaclosed { get; set; }
        public string Soano { get; set; }
        public bool? BillingGenerated { get; set; }
        public string BillingNo { get; set; }
        public string SoaadjustmentRequestor { get; set; }
        public DateTime? SoaadjustmentRequestedDate { get; set; }
        public string SoaadjustmentType { get; set; }
        public string SoaadjustmentReason { get; set; }
        public string AdjustedSoauser { get; set; }
        public DateTime? AdjustedSoadate { get; set; }
        public string UnlockedSoasaleMan { get; set; }
        public string UnlockedSoasaleManStatus { get; set; }
        public DateTime? UnlockedSoasaleManDate { get; set; }
        public string UnlockedSoadirector { get; set; }
        public string UnlockedSoadirectorStatus { get; set; }
        public DateTime? UnlockedSoadirectorDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string Sotype { get; set; }
        public string DeliveryStatus { get; set; }
        public string FailedDeliveryReason { get; set; }
        public string FailedDeliveryDueTo { get; set; }
        public DateTime? PodreceivedDate { get; set; }
        public DateTime? PodhanoverRequestDate { get; set; }
        public DateTime? PodreturnedDate { get; set; }
        public int? PodleadTime { get; set; }
        public string CustomerRouteCode { get; set; }

        public ICollection<CsDtborderDropPointItemRoute> CsDtborderDropPointItemRoute { get; set; }
    }
}
