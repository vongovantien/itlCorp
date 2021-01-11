using eFMS.API.Accounting.Service.Models;
using System;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctAdvanceRequestModel : AcctAdvanceRequest
    {
        public DateTime? RequestDate { get; set; }
        public DateTime? ApproveDate { get; set; }
        public DateTime? SettleDate { get; set; }
        public string Requester { get; set; }
        public string RequesterName { get; set; }


    }
}
