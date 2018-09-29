using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsDtborderDropPoint
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Code { get; set; }
        public DateTime? RequestDate { get; set; }
        public string RequestFromTime { get; set; }
        public string RequestToTime { get; set; }
        public DateTime? ArrivedTime { get; set; }
        public DateTime? LeftTime { get; set; }
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public string Address { get; set; }
        public Guid? WardId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? ProvinceId { get; set; }
        public string GeoCode { get; set; }
        public string PointType { get; set; }
        public string Note { get; set; }
        public int? TotalCustomerQuantity { get; set; }
        public decimal? TotalEstimateWeight { get; set; }
        public decimal? TotalEstimateVolume { get; set; }
        public decimal? TotalEstimateChargedWeight { get; set; }
        public int? TotalQuantity { get; set; }
        public decimal? TotalActualWeight { get; set; }
        public decimal? TotalActualVolume { get; set; }
        public decimal? TotalActualChargedWeight { get; set; }
        public decimal? Codvalue { get; set; }
        public decimal? IndicatedCodvalue { get; set; }
        public short? UnitId { get; set; }
        public decimal? AssignedTransportWeight { get; set; }
        public int? CurrentStatusId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
