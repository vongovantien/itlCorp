using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceDtbrateCardCondition
    {
        public Guid Id { get; set; }
        public short VehicleTypeId { get; set; }
        public decimal? DropPointPrice { get; set; }
        public decimal? DropPointStandardPrice { get; set; }
        public decimal? OutsideDropPointPrice { get; set; }
        public decimal? DiscountWeight { get; set; }
        public decimal? DiscountVolumn { get; set; }
        public decimal? Discount { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public int RateCardId { get; set; }
        public decimal? ReferCustomerFee { get; set; }
        public decimal? ReferCustomerOutsideFee { get; set; }
    }
}
