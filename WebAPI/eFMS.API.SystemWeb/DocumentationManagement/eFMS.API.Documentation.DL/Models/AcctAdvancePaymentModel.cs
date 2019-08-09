using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class AcctAdvancePaymentModel : AcctAdvancePayment
    {
        public List<AcctAdvanceRequestModel> AdvanceRequests { get; set; }
    }
}
