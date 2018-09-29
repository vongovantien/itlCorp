﻿using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsFcltransportTripRecord
    {
        public Guid Id { get; set; }
        public Guid TransportId { get; set; }
        public int Trip { get; set; }
        public string GeoCode { get; set; }
        public string Address { get; set; }
        public int NumberInOdometer { get; set; }
        public short? Status { get; set; }
        public string ContainerNo { get; set; }
        public string SealNo { get; set; }
        public string Notes { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public int? TripLength { get; set; }
        public int? TripLengthCs { get; set; }
        public string RouteType { get; set; }
        public bool? IsDeliveryPoint { get; set; }
        public decimal? FuelConsumption { get; set; }
        public decimal? PriceFuel { get; set; }
        public decimal? FuelLiter { get; set; }
        public decimal? TotalFuelCost { get; set; }
        public DateTime? TripDatetime { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public decimal? Weight { get; set; }
        public decimal? WeightReal { get; set; }
    }
}
