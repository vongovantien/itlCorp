using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CsFcltransactionDetailContainer
    {
        public Guid Id { get; set; }
        public string RequestNo { get; set; }
        public string RqTimes { get; set; }
        public DateTime? RequestDate { get; set; }
        public string Hblno { get; set; }
        public string RequestService { get; set; }
        public string CustomerId { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string PortofLoading { get; set; }
        public string PortofDischarge { get; set; }
        public bool PersonalCustomsNonTrd { get; set; }
        public bool CompanyCustomsNonTrd { get; set; }
        public bool CustomsTrading { get; set; }
        public string GoodsDescription { get; set; }
        public double? Quantity { get; set; }
        public string Unit { get; set; }
        public string Packages { get; set; }
        public double? Measurement { get; set; }
        public string GoodsNotes { get; set; }
        public string DocsRequest { get; set; }
        public string ClosingTime { get; set; }
        public string CargoContactAddress { get; set; }
        public string CargoContact { get; set; }
        public string CargoContactTel { get; set; }
        public string CargoContactTime { get; set; }
        public string CargoContactOthers { get; set; }
        public DateTime? Etd { get; set; }
        public string Notes { get; set; }
        public bool Finished { get; set; }
        public string Whoismaking { get; set; }
        public string OperationContact { get; set; }
        public string Opexecutive { get; set; }
        public bool Approved { get; set; }
        public bool Previewed { get; set; }
        public bool Decline { get; set; }
        public bool? Inland { get; set; }
        public string ContQty { get; set; }
        public string EmptyReturnPickup { get; set; }
        public string ShipperId { get; set; }
        public string ConsigneeId { get; set; }
        public decimal? IdkeyShipmentDt { get; set; }
        public string VesselVoy { get; set; }
        public string Cdsno { get; set; }
        public string Cdstype { get; set; }
        public bool? WaitH { get; set; }
        public DateTime? AppDate { get; set; }
        public string AppMode { get; set; }
        public string JobApp { get; set; }
        public bool? ForceNew { get; set; }
        public string Apptype { get; set; }
        public string NmpartyId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
