using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.ForPartner.DL.Models.Payable
{
    public class CancelPayablePayment
    {
        public string PaymentNo { get; set; }
        public string OfficeCODE { get; set; }
        public string TransactionType { get; set; }
        public string BravoRefNo { get; set; }
    }
}
