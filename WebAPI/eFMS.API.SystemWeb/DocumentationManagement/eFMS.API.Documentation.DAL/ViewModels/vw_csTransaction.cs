using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public partial class vw_csTransaction
    {
        public Guid ID { get; set; }
        public Guid BranchID { get; set; }
        public string JobNo { get; set; }
        public string MAWB { get; set; }
        public string TypeOfService { get; set; }
        public DateTime? ETD { get; set; }
        public DateTime? ETA { get; set; }
        public string MBLType { get; set; }
        public string ColoaderID { get; set; }
        public string BookingNo { get; set; }
        public string ShippingServiceType { get; set; }
        public string AgentID { get; set; }
        public Guid? POL { get; set; }
        public string POD { get; set; }
        public string PaymentTerm { get; set; }
        public DateTime? LoadingDate { get; set; }
        public DateTime? RequestedDate { get; set; }
        public string FlightVesselName { get; set; }
        public string VoyNo { get; set; }
        public DateTime? FlightVesselConfirmedDate { get; set; }
        public string ShipmentType { get; set; }
        public string ServiceMode { get; set; }
        public string Commodity { get; set; }
        public string InvoiceNo { get; set; }
        public string PONo { get; set; }
        public string PersonIncharge { get; set; }
        public string DeliveryPoint { get; set; }
        public string RouteShipment { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> Unit { get; set; }
        public Nullable<decimal> GrossWeight { get; set; }
        public Nullable<decimal> ChargeWeight { get; set; }
        public Nullable<decimal> CBM { get; set; }
        public string ContainerSize { get; set; }
        public string Dimension { get; set; }
        public string WareHouseID { get; set; }
        public string Notes { get; set; }
        public Nullable<bool> Locked { get; set; }
        public DateTime? LockedDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UserModified { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Nullable<bool> Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string SupplierName { get; set; }
        public string AgentName { get; set; }
        public string HWBNo { get; set; }
        public string CustomerID { get; set; }
        public string NotifyPartyID { get; set; }
        public string SaleManID { get; set; }
        public string SealNo { get; set; }
        public string ContainerNo { get; set; }
    }
}
