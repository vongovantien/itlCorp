using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class OpsAdvanceSettlementModel
    {
        public string Requester { get; set; }
        public string AdvanceNo { get; set; }
        public string AdvanceAmount { get; set; }
        public string StatusApproval { get; set; }
        public string SettlementNo { get; set; }
        public string SettlementAmount { get; set; }
        public string SettleStatusApproval { get; set; }
        public string Balance { get; set; }
        public DateTime? AdvanceDate { get; set; }
        public DateTime? SettlemenDate { get; set; }
    }
}
