using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsTransaction
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string JobNo { get; set; }
        public string Mawb { get; set; }
        public string TypeOfService { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string Mbltype { get; set; }
        public string ColoaderId { get; set; }
        public string SubColoader { get; set; }
        public string BookingNo { get; set; }
        public string AgentId { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public Guid? DeliveryPlace { get; set; }
        public string PaymentTerm { get; set; }
        public string FlightVesselName { get; set; }
        public string VoyNo { get; set; }
        public string ShipmentType { get; set; }
        public string Commodity { get; set; }
        public string DesOfGoods { get; set; }
        public string PackageContainer { get; set; }
        public string Pono { get; set; }
        public string PersonIncharge { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? ChargeWeight { get; set; }
        public decimal? Cbm { get; set; }
        public int? PackageQty { get; set; }
        public string PackageType { get; set; }
        public string Notes { get; set; }
        public string TransactionType { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime? LockedDate { get; set; }
        public bool? IsLocked { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
