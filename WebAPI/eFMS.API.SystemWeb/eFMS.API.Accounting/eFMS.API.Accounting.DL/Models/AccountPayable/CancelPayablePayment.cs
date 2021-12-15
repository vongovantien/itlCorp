using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountPayable
{
    public class CancelPayablePayment
    {
        public string PaymentNo { get; set; }
        public string OfficeCODE { get; set; }
        public string TransactionType { get; set; }
    }
}
