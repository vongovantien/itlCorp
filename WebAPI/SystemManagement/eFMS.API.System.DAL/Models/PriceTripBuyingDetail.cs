using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceTripBuyingDetail
    {
        public Guid Id { get; set; }
        public Guid? BuyingVehicleTypeId { get; set; }
        public Guid PlaceFrom { get; set; }
        public Guid PlaceTo { get; set; }
        public decimal? Price { get; set; }
        public decimal? DropPointPrice { get; set; }
        public decimal? OutsideDropPointPrice { get; set; }
        public string Note { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public decimal? ReferCustomerFee { get; set; }
    }
}
