using System;
using System.Collections.Generic;

namespace eFMS.API.ReportData.Models
{
    public class SOAOPSModel
    {
        public string PartnerNameVN { get; set; }
        public string BillingAddressVN { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SoaNo { get; set; }
        public List<ExportSOAOPS> exportSOAOPs { get; set; }
    }
}
