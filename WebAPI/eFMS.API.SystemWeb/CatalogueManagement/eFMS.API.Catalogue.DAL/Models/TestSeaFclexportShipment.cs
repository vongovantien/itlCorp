using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class TestSeaFclexportShipment
    {
        public string JobId { get; set; }
        public DateTime EstimatedTimeofDepature { get; set; }
        public DateTime? EstimatedTimeofArrived { get; set; }
        public string Mblno { get; set; }
        public string MasterBillOfLoadingType { get; set; }
        public string ColoaderId { get; set; }
        public string BookingNo { get; set; }
        public string ServiceType { get; set; }
        public string AgentId { get; set; }
        public Guid PortOfLoading { get; set; }
        public Guid? PortOfDestination { get; set; }
        public string Term { get; set; }
        public string VesselName { get; set; }
        public string VoyNo { get; set; }
        public string ShippingType { get; set; }
        public string PurchaseOrderNo { get; set; }
        public string PersonInChargeId { get; set; }
        public string Note { get; set; }
        public string CommoditiesDescription { get; set; }
        public string GoodsDescription { get; set; }
        public string UserCreated { get; set; }
        public DateTime DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
