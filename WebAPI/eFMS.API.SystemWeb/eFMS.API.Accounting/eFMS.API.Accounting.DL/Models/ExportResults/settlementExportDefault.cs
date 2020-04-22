using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.ExportResults
{
    public class SettlementExportDefault
    {
        public string  JobID { get; set; }
        public string  MBL { get; set; }
        public string  HBL { get; set; }
        public string  CustomNo { get; set; }
        public string  SettleNo { get; set; }
        public string  Requester { get; set; }
        public decimal? SettlementAmount { get; set; }
        public string AdvanceNo { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public string Currency { get; set; }
        public DateTime? ApproveDate { get; set; }
        public DateTime? RequestDate { get; set; }
        public string Description { get; set; }
    }
}
