using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class AccAccountReceivableCriteria
    {
        public Guid AgreementId { get; set; }
        public Guid PartnerId { get; set; }
        public Guid AgreementSalesmanId { get; set; }
    }
}
