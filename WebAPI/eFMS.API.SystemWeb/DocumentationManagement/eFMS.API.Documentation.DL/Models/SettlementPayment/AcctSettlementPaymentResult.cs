using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.SettlementPayment
{
    public class AcctSettlementPaymentResult : AcctSettlementPayment
    {
        public decimal? Amount { get; set; }
        public string RequesterName { get; set; }
        public string PaymentMethodName { get; set; }        
        public string StatusApprovalName { get; set; }
        public string ChargeCurrency { get; set; }
    }
}
