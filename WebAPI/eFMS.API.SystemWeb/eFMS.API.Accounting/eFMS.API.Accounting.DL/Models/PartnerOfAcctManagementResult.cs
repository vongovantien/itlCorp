using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class PartnerOfAcctManagementResult
    {
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string PartnerAddress { get; set; }
        public string SettlementRequesterId { get; set; }
        public string SettlementRequester { get; set; }
        public string InputRefNo { get; set; }
        public string Service { get; set; }
        public List<ChargeOfAccountingManagementModel> Charges;
    }
}
