using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class CsTransaction
    {
        public CsTransaction()
        {
            CsTransactionDetail = new HashSet<CsTransactionDetail>();
        }

        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string JobNo { get; set; }
        public string Mawb { get; set; }
        public string TypeOfService { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public string Mbltype { get; set; }
        public string ColoaderId { get; set; }
        public string BookingNo { get; set; }
        public string ShippingServiceType { get; set; }
        public string AgentId { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public string PaymentTerm { get; set; }
        public DateTime? LoadingDate { get; set; }
        public DateTime? RequestedDate { get; set; }
        public string FlightVesselName { get; set; }
        public string VoyNo { get; set; }
        public DateTime? FlightVesselConfirmedDate { get; set; }
        public string ShipmentType { get; set; }
        public string ServiceMode { get; set; }
        public string Commodity { get; set; }
        public string DesOfGoods { get; set; }
        public string PackageContainer { get; set; }
        public string InvoiceNo { get; set; }
        public string Pono { get; set; }
        public string PersonIncharge { get; set; }
        public string DeliveryPoint { get; set; }
        public string RouteShipment { get; set; }
        public int? Quantity { get; set; }
        public int? Unit { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? ChargeWeight { get; set; }
        public decimal? Cbm { get; set; }
        public string ContainerSize { get; set; }
        public string Dimension { get; set; }
        public string WareHouseId { get; set; }
        public string Notes { get; set; }
        public bool? Locked { get; set; }
        public DateTime? LockedDate { get; set; }
        public string TransactionType { get; set; }
        public string UserCreated { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UserModified { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual ICollection<CsTransactionDetail> CsTransactionDetail { get; set; }
    }
}
