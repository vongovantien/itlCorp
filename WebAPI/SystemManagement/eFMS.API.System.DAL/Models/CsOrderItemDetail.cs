using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsOrderItemDetail
    {
        public Guid Id { get; set; }
        public Guid OrderDetailId { get; set; }
        public int CustomerQuantity { get; set; }
        public string Sku { get; set; }
        public string Model { get; set; }
        public decimal? EstimateWeight { get; set; }
        public decimal? EstimateLength { get; set; }
        public decimal? EstimateWidth { get; set; }
        public decimal? EstimateHeight { get; set; }
        public decimal? EstimateVolume { get; set; }
        public decimal? EstimateChargeableWeight { get; set; }
        public int Quantity { get; set; }
        public int? BaggedQuantity { get; set; }
        public decimal? ActualWeight { get; set; }
        public decimal? ActualLength { get; set; }
        public decimal? ActualWidth { get; set; }
        public decimal? ActualHeight { get; set; }
        public decimal? ActualVolume { get; set; }
        public decimal? ActualChargeableWeight { get; set; }
        public string ItemDescription { get; set; }
        public bool? IsAdditional { get; set; }
        public bool? Reweighed { get; set; }
        public string ReweighedBy { get; set; }
        public DateTime? ReweighedOn { get; set; }
        public string Remark { get; set; }
        public string OriginHubRemark { get; set; }
        public string DestHubRemark { get; set; }
        public string DestBranchRemark { get; set; }
        public string ReProtectedUser { get; set; }
        public DateTime? ReProtectedDate { get; set; }
        public byte[] Version { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public decimal? BaggedWeight { get; set; }
        public decimal? BaggedVolume { get; set; }
    }
}
