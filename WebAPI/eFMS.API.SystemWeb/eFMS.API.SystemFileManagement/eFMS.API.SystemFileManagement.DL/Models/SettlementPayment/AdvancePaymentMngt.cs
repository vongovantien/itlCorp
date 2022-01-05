using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.SettlementPayment
{
    public class AdvancePaymentMngt
    {
        public string AdvanceNo { get; set; }
        public string Description { get; set; }
        public decimal TotalAmount { get; set; }
        public string AdvanceCurrency { get; set; }
        public DateTime? AdvanceDate { get; set; }
        public List<ChargeAdvancePaymentMngt> ChargeAdvancePaymentMngts { get; set; }
    }

    public class ChargeAdvancePaymentMngt
    {
        public string AdvanceNo { get; set; }
        public string Description { get; set; }
        public decimal TotalAmount { get; set; }
        public string AdvanceCurrency { get; set; }
    }
}
