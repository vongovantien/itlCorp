using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.ReportResults
{
    public class ShippingInstructionReportResult
    {
        //public decimal SumVolume { get; set; }
        //public decimal SumGrossWeight { get; set; }
        //public string SumPackagesNote { get; set; }
        //public string SumContainerSealNo { get; set; }
        public string VesselNo { get; set; }
        public string PoDelivery { get; set; }
        public string PodName { get; set; }
        public string PolName { get; set; }
        public string Remark { get; set; }
        public string CargoNoticeRecevier { get; set; }
        public string Shipper { get; set; }
        public string LoadingDate { get; set; }
        public string InvoiceDate { get; set; }
        public string BookingNo { get; set; }
        public string InvoiceNoticeRecevier { get; set; }
        public string SupplierName { get; set; }
        public string IssuedUserTel { get; set; }
        public string IssuedUserName { get; set; }
        public string ConsigneeDescription { get; set; }
        public string PaymenType { get; set; }
    }
    public class ShippingInstructionContainer
    {
        public string ContainerTypesNote { get; set; }
        public string ContainerSealNo { get; set; }
        public string PackagesNote { get; set; }
        public string DesOfGoods { get; set; }
        public decimal? GW { get; set; }
        public decimal? CBM { get; set; }
        public string SumContainerSealNo { get; set; }
        public string SumPackagesNote { get; set; }
        public decimal? SumGrossWeight { get; set; }
        public decimal? SumVolume { get; set; }
        public string Remark { get; set; }	
        public string Payment { get; set; }		
    }
    public class ContainerObject
    {
        public int Quantity { get; set; }
        public string Name { get; set; }
    }
}
