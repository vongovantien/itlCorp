using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SaleQuotationRoute
    {
        public Guid Id { get; set; }
        public Guid QuotationId { get; set; }
        public string PickupAdress { get; set; }
        public Guid? PlaceFrom { get; set; }
        public Guid? PlaceTo { get; set; }
        public string DeliveryAddress { get; set; }
        public Guid? OriginBranchId { get; set; }
        public Guid? OriginHubId { get; set; }
        public Guid? DestinationBranchId { get; set; }
        public Guid? DestinationHubId { get; set; }
        public short? PickupZoneId { get; set; }
        public short? DeliveryZoneId { get; set; }
        public int? CommodityId { get; set; }
        public decimal? EstimateWeight { get; set; }
        public int? ServiceTypeId { get; set; }
        public decimal? Kratio { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }

        public SaleQuotation Quotation { get; set; }
    }
}
