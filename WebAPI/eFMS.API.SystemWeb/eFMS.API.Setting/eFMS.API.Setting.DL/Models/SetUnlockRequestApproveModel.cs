using eFMS.API.Setting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class SetUnlockRequestApproveModel : SetUnlockRequestApprove
    {
        public string LeaderName { get; set; }
        public string ManagerName { get; set; }
        public string AccountantName { get; set; }
        public string BUHeadName { get; set; }
        public bool IsApproved { get; set; }
        public string StatusApproval { get; set; }
        public int NumOfDeny { get; set; }
        public bool IsShowLeader { get; set; }
        public bool IsShowManager { get; set; }
        public bool IsShowAccountant { get; set; }
        public bool IsShowBuHead { get; set; }
    }
}
