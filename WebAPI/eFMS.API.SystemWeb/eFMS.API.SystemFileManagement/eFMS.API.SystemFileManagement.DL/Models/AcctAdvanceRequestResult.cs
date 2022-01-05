using eFMS.API.SystemFileManagement.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class AcctAdvanceRequestResult : AcctAdvanceRequest
    {
        public string Requester { get; set; }
        public string RequesterName { get; set; }
        public string Department { get; set; }
        public string PaymentMethod { get; set; }
        public string AdvanceCurrency { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DeadlinePayment { get; set; }
        public string StatusApproval { get; set; }
        public string AdvanceNote { get; set; }
        public DateTime? AdvanceDatetimeModified { get; set; }
    }
}
