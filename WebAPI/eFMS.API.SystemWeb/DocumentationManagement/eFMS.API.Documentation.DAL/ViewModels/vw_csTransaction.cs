using System;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public partial class vw_csTransaction
    {
        public Guid ID { get; set; }
        public Guid BranchID { get; set; }
        public string JobNo { get; set; }
        public string MAWB { get; set; }
        public string TypeOfService { get; set; }
        public Nullable<DateTime> ETD { get; set; }
        public Nullable<DateTime> ETA { get; set; }
        public string MBLType { get; set; }
        public string ColoaderID { get; set; }
        public string BookingNo { get; set; }
        public string ShippingServiceType { get; set; }
        public string AgentID { get; set; }
        public Nullable<Guid> POL { get; set; }
        public Nullable<Guid> POD { get; set; }
        public string PaymentTerm { get; set; }
        public Nullable<DateTime> LoadingDate { get; set; }
        public Nullable<DateTime> RequestedDate { get; set; }
        public string FlightVesselName { get; set; }
        public string VoyNo { get; set; }
        //public Nullable<System.DateTime> FlightVesselConfirmedDate { get; set; }
        public string ShipmentType { get; set; }
        public string ServiceMode { get; set; }
        public string Commodity { get; set; }
        public string DesOfGoods { get; set; }
        public string PackageContainer { get; set; }
        public string InvoiceNo { get; set; }
        public string PONo { get; set; }
        public string PersonIncharge { get; set; }
        public string DeliveryPoint { get; set; }
        public string RouteShipment { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> Unit { get; set; }
        public Nullable<decimal> NetWeight { get; set; }
        public Nullable<decimal> GrossWeight { get; set; }
        public Nullable<decimal> ChargeWeight { get; set; }
        public Nullable<decimal> CBM { get; set; }
        public string ContainerSize { get; set; }
        public string Dimension { get; set; }
        public string WareHouseID { get; set; }
        public string Notes { get; set; }
        public Nullable<bool> Locked { get; set; }
        public string LockedDate { get; set; }
        //public Nullable<System.DateTime> LockedDate { get; set; }
        public string UserCreated { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public string UserModified { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public Nullable<bool> Active { get; set; }
        //public Nullable<System.DateTime> InactiveOn { get; set; }
        public string InactiveOn { get; set; }
        public string SupplierName { get; set; }
        public string AgentName { get; set; }
        public string HWBNo { get; set; }
        public string CustomerID { get; set; }
        public string NotifyPartyID { get; set; }
        public string SaleManID { get; set; }
        public string PODName { get; set; }
        public string POLName { get; set; }
        public string CreatorName { get; set; }
        public Nullable<int> SumCont { get; set; }
        public Nullable<decimal> SumCBM { get; set; }
    }
}
