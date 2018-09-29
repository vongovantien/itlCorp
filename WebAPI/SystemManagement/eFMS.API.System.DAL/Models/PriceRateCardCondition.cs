using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceRateCardCondition
    {
        public PriceRateCardCondition()
        {
            PriceRateCardDetail = new HashSet<PriceRateCardDetail>();
            PriceRateCardOverWeightDetail = new HashSet<PriceRateCardOverWeightDetail>();
        }

        public Guid Id { get; set; }
        public int RateCardId { get; set; }
        public int? ServiceTypeMappingId { get; set; }
        public short PickupZoneCode { get; set; }
        public Guid? PickupPlaceId { get; set; }
        public short DeliveryZoneCode { get; set; }
        public Guid? DeliveryPlaceId { get; set; }
        public string RoadId { get; set; }
        public int? CommodityId { get; set; }
        public string RouteType { get; set; }
        public int? SequentialNumber { get; set; }
        public Guid? TripBuyingRouteId { get; set; }
        public Guid? FclbuyingId { get; set; }
        public Guid? BuyingRouteId { get; set; }
        public Guid? PriceSellingId { get; set; }

        public PriceRateCard RateCard { get; set; }
        public ICollection<PriceRateCardDetail> PriceRateCardDetail { get; set; }
        public ICollection<PriceRateCardOverWeightDetail> PriceRateCardOverWeightDetail { get; set; }
    }
}
