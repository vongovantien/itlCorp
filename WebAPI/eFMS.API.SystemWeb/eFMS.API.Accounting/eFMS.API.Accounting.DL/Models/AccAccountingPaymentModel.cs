﻿using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class AccAccountingPaymentModel: AccAccountingPayment
    {
        public string UserModifiedName { get; set; }
        public string RefNo { get; set; }
        public string ReceiptNo { get; set; }
    }
}
