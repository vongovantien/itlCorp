using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class ExportCombineShipment
    {
        public string CustomDeclarationNo { get; set; }
        public string JobNo { get; set; }
        public string CommodityName { get; set; }
        public string HwbNo { get; set; }
        public decimal? GW { get; set; }
        public decimal? CBM { get; set; }
        public string PackageContainer { get; set; }
        public string CombineNo { get; set; }
        public string BillingNo { get; set; }
        public decimal? KGS { get; set; }
        public decimal? CusFee { get; set; }
        public decimal? CusVAT { get; set; }
        public decimal? AuthFee { get; set; }
        public decimal? AuthVAT { get; set; }
        public string FreInvoice { get; set; }
        public decimal? FreFee { get; set; }
        public decimal? FreVAT { get; set; }
        public string InvoiceNo { get; set; }

    }
}
