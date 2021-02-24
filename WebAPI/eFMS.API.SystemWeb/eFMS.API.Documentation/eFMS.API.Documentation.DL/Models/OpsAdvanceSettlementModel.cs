using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class OpsAdvanceSettlementModel
    {
        public string JobNo { get; set; }
        public string RequesterAdvance { get; set; }
        public string RequesterSettle { get; set; }
        public string AdvanceNo { get; set; }
        public decimal AdvanceAmount { get; set; }
        public string AdvanceStatusApproval { get; set; }
        public string SettlementNo { get; set; }
        public decimal SettlementAmount { get; set; }
        public string SettleStatusApproval { get; set; }
        public decimal Balance { get; set; }
        public DateTime? AdvanceDate { get; set; }
        public DateTime? SettlemenDate { get; set; }
        public string AdvanceCurrency { get; set; }
        public string SettlementCurrency { get; set; }
    }
}
