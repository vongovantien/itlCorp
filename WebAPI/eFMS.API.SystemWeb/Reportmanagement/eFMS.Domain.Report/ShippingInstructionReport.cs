using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.Domain.Report
{
    public class ShippingInstructionReport
    {
        public string IssuedUserName { get; set; }
        public string IssuedUserTel { get; set; }
        public string SupplierName { get; set; }
        public string InvoiceNoticeRecevier { get; set; }
        public string BookingNo { get; set; }
        public string InvoiceDate { get; set; }
        public string LoadingDate { get; set; }
        public string Shipper { get; set; }
        public string ConsigneeDescription { get; set; }
        public string CargoNoticeRecevier { get; set; }
        public string PolName { get; set; }
        public string PodName { get; set; }
        public string PoDelivery { get; set; }
        public string VesselNo { get; set; }
        public string SumContainerSealNo { get; set; }
        public string SumPackagesNote { get; set; }
        public decimal SumGrossWeight { get; set; }
        public decimal SumVolume { get; set; }
        public string Remark { get; set; }
        public string PaymenType { get; set; }
    }
}
