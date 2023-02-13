using System;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class EDocFile
    {
        public string BillingNo { get; set; }
        public string BillingType { get; set; }
        public string BillingId { get; set; }
        public Guid? HBL { get; set; }
        public Guid? JobId { get; set; }
        public string Code { get; set; }
        public int DocumentId { get; set; }
        public string TransactionType { get; set; }
        public string AliasName { get; set; }
        public string FileName { get; set; }
        public string Note { get; set; }
        public string AccountingType { get; set; }
    }
    
}
