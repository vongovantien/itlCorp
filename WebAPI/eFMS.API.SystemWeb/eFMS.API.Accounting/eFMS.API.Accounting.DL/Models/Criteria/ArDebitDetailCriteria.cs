using eFMS.API.Accounting.DL.Common;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class ArDebitDetailCriteria
    {
        public Guid ArgeementId { get; set; }
        public string PartnerId { get; set; }
        public string Option { get; set; }
        public string OfficeId { get; set; }
        public string ServiceCode { get; set; }
        public int OverDueDay { get; set; }
        public string ArSalesManId { get; set; }
    }
}
