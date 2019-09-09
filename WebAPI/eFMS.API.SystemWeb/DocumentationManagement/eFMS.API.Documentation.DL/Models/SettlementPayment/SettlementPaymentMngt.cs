using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.SettlementPayment
{
    public class SettlementPaymentMngt
    {
        public string SettlementNo { get; set; }
        public string ChargeName { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencySettlement { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string OBHPartner { get; set; }
    }
}
