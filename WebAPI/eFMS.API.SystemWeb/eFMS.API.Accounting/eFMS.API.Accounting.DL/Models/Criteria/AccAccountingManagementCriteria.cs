using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class AccAccountingManagementCriteria
    {
        public List<string> ReferenceNos { get; set; }
        public string PartnerId { get; set; }
        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }
        public string CreatorId { get; set; }
        public string InvoiceStatus { get; set; }
        public string VoucherType { get; set; }
        public string TypeOfAcctManagement { get; set; }
    }
}
