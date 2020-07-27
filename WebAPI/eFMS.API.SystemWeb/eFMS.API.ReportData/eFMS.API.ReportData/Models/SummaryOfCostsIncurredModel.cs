using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class SummaryOfCostsIncurredModel
    {
        public string SupplierCode { get; set; }
        public string SuplierName { get; set; }
        public string POLName { get; set; }
        public string PurchaseOrderNo { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string CustomNo { get; set; }
        public string Type { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string Currency { get; set; }
        public string InvoiceNo { get; set; }
        public string Note { get; set; }
        public string TransactionType { get; set; }
        public string UserCreated { get; set; }
        public decimal Quantity { get; set; }
        public short UnitId { get; set; }
        public string Unit { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? VATRate { get; set; }
        public string Commodity { get; set; }

        public string Service { get; set; }
        public string CDNote { get; set; }
        public string TaxCodeOBH { get; set; }

        public decimal? GrossWeight { get; set; }
        public decimal? ChargeWeight { get; set; }
        public decimal? CBM { get; set; }

        public int? PackageQty { get; set; }

        public string PackageContainer { get; set; }

        public string TypeCharge { get; set; }
        public decimal? VATAmount { get; set; }
        public decimal? NetAmount { get; set; }
    }
}
