using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.SettlementPayment
{
    public class SettlementPaymentMngt
    {
        public string SettlementNo { get; set; }
        public string ChargeName { get; set; }
        public decimal TotalAmount { get; set; }
        public string SettlementCurrency { get; set; }
        public string ChargeCurrency { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string OBHPartner { get; set; }
        public string Payer { get; set; }

        public List<ChargeSettlementPaymentMngt> ChargeSettlementPaymentMngts { get; set; }
    }

    public class ChargeSettlementPaymentMngt
    {
        public string SettlementNo { get; set; }
        public string AdvanceNo { get; set; }
        public string ChargeName { get; set; }
        public decimal TotalAmount { get; set; }
        public string SettlementCurrency { get; set; }
        public string OBHPartner { get; set; }
        public string Payer { get; set; }
    }
}
