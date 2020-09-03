using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctAdvancePaymentModel : AcctAdvancePayment
    {
        public List<AcctAdvanceRequestModel> AdvanceRequests { get; set; }
        public int NumberOfRequests { get; set; }
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
        public string RequesterName { get; set; }
        public List<string> advancePaymentIds { get; set; }
        public bool IsRequester { get; set; }
        public bool IsManager { get; set; }
        public bool IsApproved { get; set; }
        public bool IsShowBtnDeny { get; set; }
    }
}
