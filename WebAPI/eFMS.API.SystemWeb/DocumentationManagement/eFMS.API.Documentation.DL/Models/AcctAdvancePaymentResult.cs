using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class AcctAdvancePaymentResult : AcctAdvancePayment
    {
        public string RequesterName { get; set; }
        public string AdvanceStatusPayment { get; set; }
        public decimal? Amount { get; set; }
        public string StatusApprovalName { get; set; }
    }
}
