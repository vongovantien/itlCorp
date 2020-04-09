using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models.Accounting
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
