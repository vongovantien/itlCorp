﻿using eFMS.API.SystemFileManagement.Service.Models;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class AcctApproveAdvanceModel : AcctApproveAdvance
    {
        public string RequesterName { get; set; }
        public string LeaderName { get; set; }
        public string ManagerName { get; set; }
        public string AccountantName { get; set; }
        public string BUHeadName { get; set; }
        public string StatusApproval { get; set; }
        public int NumOfDeny { get; set; }
        public bool IsShowLeader { get; set; }
        public bool IsShowManager { get; set; }
        public bool IsShowAccountant { get; set; }
        public bool IsShowBuHead { get; set; }
    }
}
