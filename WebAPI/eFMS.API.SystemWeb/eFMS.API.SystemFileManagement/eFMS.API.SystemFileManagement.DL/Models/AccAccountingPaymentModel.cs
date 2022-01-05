using eFMS.API.SystemFileManagement.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class AccAccountingPaymentModel: AccAccountingPayment
    {
        public string UserModifiedName { get; set; }
        public string RefNo { get; set; }
        public string ReceiptNo { get; set; }
    }
}
