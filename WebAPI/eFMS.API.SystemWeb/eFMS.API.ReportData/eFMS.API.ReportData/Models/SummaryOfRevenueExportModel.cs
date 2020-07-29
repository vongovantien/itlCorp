using eFMS.API.ReportData.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class SummaryOfRevenueExportModel
    {
        public string SupplierCode { get; set; }
        public string SuplierName { get; set; }
        public string POLName { get; set; }
        public string PurchaseOrderNo { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string CustomNo { get; set; }
        public decimal? CBM { get; set; }
        public decimal? GrossWeight { get; set; }
        public string PackageContainer { get; set; }

        public List<SummaryOfCostsIncurredModel> SummaryOfCostsIncurredExportResults { get; set; }
    }
}
