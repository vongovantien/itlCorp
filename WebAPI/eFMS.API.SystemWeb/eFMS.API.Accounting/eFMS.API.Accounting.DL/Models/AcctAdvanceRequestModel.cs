using eFMS.API.Accounting.DL.Models.AdvancePayment;
using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctAdvanceRequestModel : AcctAdvanceRequest
    {
        public List<ShipmentChargeAdvance> Surcharge { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? ApproveDate { get; set; }
        public DateTime? SettleDate { get; set; }
        public string Requester { get; set; }
        public string RequesterName { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? DeadlinePayment { get; set; }    
        public string BankAccountNo { get; set; }
        public string BankAccountName { get; set; }
        public string BankName { get; set; }

    }
}
