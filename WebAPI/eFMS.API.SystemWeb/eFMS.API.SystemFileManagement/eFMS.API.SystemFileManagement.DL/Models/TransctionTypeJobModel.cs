using System;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class TransctionTypeJobModel
    {
        public Guid JobId { get; set; }
        public string TransactionType { get; set; }
        public string BillingNo { get; set; }
        public string Code { get; set; }
    }
}
