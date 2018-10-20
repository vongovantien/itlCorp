using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class CsShipment
    {
        public string BranchId { get; set; }
        public string JobId { get; set; }
        public string TypeOfService { get; set; }
        public DateTime LoadingDate { get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? FlightVesselDateConfirm { get; set; }
        public string Mawb { get; set; }
        public string ColoaderId { get; set; }
        public string AgentId { get; set; }
        public bool? ShipmentType { get; set; }
        public string PaymentTerm { get; set; }
        public string ServiceMode { get; set; }
        public string Commodity { get; set; }
        public string FlightVesselNo { get; set; }
        public string Voy { get; set; }
        public string InvoiceNo { get; set; }
        public string Po { get; set; }
        public string PortofLadingId { get; set; }
        public string PortofUnladingId { get; set; }
        public string DeliveryPoint { get; set; }
        public string RouteShipment { get; set; }
        public double? Qty { get; set; }
        public string Unit { get; set; }
        public double? GrossWeight { get; set; }
        public double? ChargeWeight { get; set; }
        public double? Cbm { get; set; }
        public string ContainerSize { get; set; }
        public string Dimension { get; set; }
        public string Mbltype { get; set; }
        public string Opic { get; set; }
        public string WareHouseId { get; set; }
        public string CargoOp { get; set; }
        public string Notes { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? ShipmentLockedStatus { get; set; }
        public DateTime? ShipmentLockedDate { get; set; }
    }
}
