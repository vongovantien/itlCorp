using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class AcctApproveAdvanceModel : AcctApproveAdvance
    {
        public string RequesterName { get; set; }
        public string LeaderName { get; set; }
        public string ManagerName { get; set; }
        public string AccountantName { get; set; }
        public string BUHeadName { get; set; }    
        public bool IsApproved { get; set; }
    }
}
