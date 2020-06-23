using eFMS.API.Accounting.DL.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountingPayment
{
    public class ExtendDateUpdatedModel
    {
        public string RefId { get; set; }
        public int NumberDaysExtend { get; set; }
        public string Note { get; set; }
        public PaymentType PaymentType { get; set; }
    }
}
