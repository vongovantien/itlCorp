using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.ExportResults
{
    public class SettlementExportGroupDefault
    {
        public string JobID { get; set; }
        public string MBL { get; set; }
        public string HBL { get; set; }
        public string CustomNo { get; set; }
        public decimal? SettlementTotalAmount { get; set; }
        public decimal? AdvanceTotalAmount { get; set; }
        public decimal? BalanceTotalAmount { get; set; }

        public List<SettlementExportDefault> requestList { get; set; }

    }
}
