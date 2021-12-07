using eFMS.API.Accounting.DL.Models;
using System;
using System.Collections.Generic;

namespace eFMS.API.ReportData.Models
{
    public class CombineOPSModel
    {
        public string PartnerNameVN { get; set; }
        public string BillingAddressVN { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string No { get; set; }
        public List<ExportCombineOPS> exportOPS { get; set; }
    }
}
