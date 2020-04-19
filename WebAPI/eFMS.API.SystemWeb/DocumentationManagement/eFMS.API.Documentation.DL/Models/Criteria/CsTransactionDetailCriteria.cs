using eFMS.API.Documentation.DL.Common;
using System;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class CsTransactionDetailCriteria
    {
        public string All { get; set; }
        public Guid? JobId { get; set; }
        public string Hwbno { get; set; }
        public string Mawb { get; set; }
        public string CustomerName { get; set; }
        public string SaleManName { get; set; }
        public string TypeFCL { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? Id { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }

        public string CustomerId { get; set; }
        public string SaleManId { get; set; }
        public string CreditDebitNo { get; set; }
        public string SoaNo { get; set; }
    }
}
