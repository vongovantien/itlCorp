﻿using eFMS.API.Accounting.Service.Models;

namespace eFMS.API.Accounting.DL.Models.SettlementPayment
{
    public class AcctApproveSettlementModel : AcctApproveSettlement
    {
        public string RequesterName { get; set; }
        public string LeaderName { get; set; }
        public string ManagerName { get; set; }
        public string AccountantName { get; set; }
        public string BUHeadName { get; set; }
        public bool IsApproved { get; set; }
        public string StatusApproval { get; set; }
        public bool IsRequester { get; set; }
    }
}
