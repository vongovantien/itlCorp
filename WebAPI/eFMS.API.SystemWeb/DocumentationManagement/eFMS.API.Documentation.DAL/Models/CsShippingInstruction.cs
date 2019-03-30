using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsShippingInstruction
    {
        public Guid JobId { get; set; }
        public string BookingNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string IssuedUser { get; set; }
        public string Supplier { get; set; }
        public string InvoiceNoticeRecevier { get; set; }
        public string Shipper { get; set; }
        public string ConsigneeId { get; set; }
        public string CargoNoticeRecevier { get; set; }
        public string ActualShipperId { get; set; }
        public string ActualConsigneeId { get; set; }
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
    }
}
