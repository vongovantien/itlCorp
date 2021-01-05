using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using System;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class CsTransactionCriteria
    {
        public string All { get; set; }
        public string JobNo { get; set; }
        public string MAWB { get; set; }
        public string HWBNo { get; set; }
        public string SupplierName { get; set; }
        public string AgentName { get; set; }
        public string CustomerId { get; set; }
        public string NotifyPartyId { get; set; }
        public string SaleManId { get; set; }
        public string SealNo { get; set; }
        public string ContainerNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }
        public string MarkNo { get; set; }
        public string CreditDebitNo { get; set; }
        public string SoaNo { get; set; }
        public string ColoaderId { get; set; }
        public string AgentId { get; set; }
        public string UserCreated { get; set; }
        public PermissionRange RangeSearch { get; set; }
        public DateTime? FromServiceDate { get; set; }
        public DateTime? ToServiceDate { get; set; }
    }
}
