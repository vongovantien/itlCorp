using System;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class EDocFile
    {
        public string BillingNo { get; set; }
        public string BillingType { get; set; }
        public string HBL { get; set; }
        public Guid JobId { get; set; }
        public string Code { get; set; }
        public string TransactionType { get; set; }
        public string AliasName { get; set; }
        public string FileName { get; set; }
    }
}
