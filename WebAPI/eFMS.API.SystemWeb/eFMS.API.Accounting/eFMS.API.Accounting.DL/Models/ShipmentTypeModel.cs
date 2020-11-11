using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class ShipmentTypeModel
    {
        public string JobNo { get; set; }
        public string TransactionType { get; set; }
        public bool? isCheckedCreditRate { get; set; }
        public bool? isCheckedPaymentTerm { get; set; }
        public bool? isCheckedExpiredDate { get; set; }
    }
}
