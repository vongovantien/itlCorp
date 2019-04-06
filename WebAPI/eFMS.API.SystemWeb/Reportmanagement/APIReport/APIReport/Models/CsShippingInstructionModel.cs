using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIReport.Models
{
    public class CsShippingInstructionModel
    {
        public Guid JobId { get; set; }
        public string BookingNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string IssuedUser { get; set; }
        public string Supplier { get; set; }
        public string InvoiceNoticeRecevier { get; set; }
        public string Shipper { get; set; }
        public string ConsigneeId { get; set; }
        public string ConsigneeDescription { get; set; }
        public string CargoNoticeRecevier { get; set; }
        public string ActualShipperId { get; set; }
        public string ActualShipperDescription { get; set; }
        public string ActualConsigneeId { get; set; }
        public string ActualConsigneeDescription { get; set; }
        public string PaymenType { get; set; }
        public string Remark { get; set; }
        public string RouteInfo { get; set; }
        public Guid? Pol { get; set; }
        public DateTime? LoadingDate { get; set; }
        public Guid? Pod { get; set; }
        public string PoDelivery { get; set; }
        public string VoyNo { get; set; }
        public string ContainerSealNo { get; set; }
        public string GoodsDescription { get; set; }
        public string ContainerNote { get; set; }
        public string PackagesNote { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? Volume { get; set; }
        public string UserCreated { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UserModified { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string IssuedUserName { get; set; }
        public string SupplierName { get; set; }
        public string ConsigneeName { get; set; }
        public string ActualConsigneeName { get; set; }
        public string ActualShipperName { get; set; }
        public string PolName { get; set; }
        public string PodName { get; set; }
        public List<TransactionDetailModel> CsTransactionDetails { get; set; }
        public List<CsMawbcontainerModel> CsMawbcontainers { get; set; }
    }
}