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
    }
}
