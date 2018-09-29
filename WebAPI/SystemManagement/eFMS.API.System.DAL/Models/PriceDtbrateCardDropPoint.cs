using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceDtbrateCardDropPoint
    {
        public Guid Id { get; set; }
        public Guid? RateCardDetailId { get; set; }
        public string Address { get; set; }
        public Guid? WardId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? ProvinceId { get; set; }
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public string GeoCode { get; set; }
        public string Note { get; set; }
        public string PointType { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
