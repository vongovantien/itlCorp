using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models.Criteria
{
    public class AccAccountingManagementCriteria
    {
        public List<string> ReferenceNos { get; set; }
        public string PartnerId { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string CreatorId { get; set; }
        public string InvoiceStatus { get; set; }
        public string VoucherType { get; set; }
        public string TypeOfAcctManagement { get; set; }
    }
}
