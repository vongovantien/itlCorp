using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models.Documentation
{
    public class CsTransactionDetailModel
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string Mawb { get; set; }
        public string Hwbno { get; set; }
        public string Hbltype { get; set; }
        public string CustomerId { get; set; }
        public string SaleManId { get; set; }
        public string ShipperDescription { get; set; }
        public string ShipperId { get; set; }
        public string ConsigneeDescription { get; set; }
        public string ConsigneeId { get; set; }
        public string NotifyPartyDescription { get; set; }
        public string NotifyPartyId { get; set; }
        public string AlsoNotifyPartyDescription { get; set; }
        public string AlsoNotifyPartyId { get; set; }
        public string CustomsBookingNo { get; set; }
        public string LocalVoyNo { get; set; }
        public string LocalVessel { get; set; }
        public string OceanVoyNo { get; set; }
        public string OceanVessel { get; set; }
        public short? OriginCountryId { get; set; }
        public string PickupPlace { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public DateTime? ShipmentEta { get; set; }
        public Guid Pol { get; set; }
        public Guid Pod { get; set; }
        public string DeliveryPlace { get; set; }
        public string FinalDestinationPlace { get; set; }
        public string ColoaderId { get; set; }
        public string FreightPayment { get; set; }
        public string PlaceFreightPay { get; set; }
        public DateTime? ClosingDate { get; set; }
        public DateTime? SailingDate { get; set; }
        public string ForwardingAgentDescription { get; set; }
        public string ForwardingAgentId { get; set; }
        public string GoodsDeliveryDescription { get; set; }
        public string GoodsDeliveryId { get; set; }
        public int? OriginBlnumber { get; set; }
        public string IssueHblplace { get; set; }
        public DateTime? IssueHbldate { get; set; }
        public string ReferenceNo { get; set; }
        public string ExportReferenceNo { get; set; }
        public string MoveType { get; set; }
        public string PurchaseOrderNo { get; set; }
        public string ServiceType { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string DocumentNo { get; set; }
        public DateTime? Etawarehouse { get; set; }
        public string WarehouseNotice { get; set; }
        public string ShippingMark { get; set; }
        public string Remark { get; set; }
        public string Commodity { get; set; }
        public string PackageContainer { get; set; }
        public string DesOfGoods { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? ChargeWeight { get; set; }
        public decimal? Cbm { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string InWord { get; set; }
        public string OnBoardStatus { get; set; }
        public string ManifestRefNo { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string ArrivalNo { get; set; }
        public DateTime? ArrivalFirstNotice { get; set; }
        public DateTime? ArrivalSecondNotice { get; set; }
        public string ArrivalHeader { get; set; }
        public string ArrivalFooter { get; set; }
        public string DeliveryOrderNo { get; set; }
        public DateTime? DeliveryOrderPrintedDate { get; set; }
        public string DosentTo1 { get; set; }
        public string DosentTo2 { get; set; }
        public string Dofooter { get; set; }
        public List<CsMawbcontainerModel> CsMawbcontainers { get; set; }
        public string CustomerName { get; set; }
        public string SaleManName { get; set; }
        public string CustomerNameVn { get; set; }
        public string SaleManNameVn { get; set; }
        public string ForwardingAgentName { get; set; }
        public string NotifyParty { get; set; }
        public string POLCode { get; set; }
        public string PODCode { get; set; }
        public string PODName { get; set; }
        public string POLName { get; set; }
        public string ContainerNames { get; set; }
        public string PackageTypes { get; set; }
        public decimal? CBM { get; set; }
        public decimal? CW { get; set; }
        public decimal? GW { get; set; }
        public string Packages { get; set; }
        public string Containers { get; set; }
        public int? PackageQty { get; set; }
        public string PackageTypeName { get; set; }
    }
}
