using System;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class sp_GetAdvanceSettleOpsTransaction
    {
        public string JobNo { get; set; }
        public string SettlementCode { get; set; }
        public decimal SettlementAmount { get; set; }
        public string SettlementCurrency { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string SettleStatusApproval { get; set; }
        public string SettleRequester { get; set; }
        public string AdvanceNo { get; set; }
        public decimal AdvanceAmount { get; set; }
        public DateTime? AdvanceDate { get; set; }
        public string AdvanceCurrency { get; set; }
        public string AdvanceStatusApproval { get; set; }
        public string AdvRequester { get; set; }
    }
}
