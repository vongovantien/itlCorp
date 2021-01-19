using eFMS.API.Documentation.DL.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class RecentlyChargeCriteria
    {
        public Guid JobId { get; set; }
        public Guid HblId { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }
        public string CustomerId { get; set; } // HBL
        public string AgentId { get; set; }    // MBL
        public string ChargeType { get; set; } // BUY/SEL
        public string ColoaderId { get; set; } // MBL
    }
}
