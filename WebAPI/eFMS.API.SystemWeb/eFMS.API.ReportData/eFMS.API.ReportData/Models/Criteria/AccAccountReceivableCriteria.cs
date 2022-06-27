using System;

namespace eFMS.API.ReportData.Models.Criteria
{
    public class AccAccountReceivableCriteria
    {
        public Guid AgreementId { get; set; }
        public Guid PartnerId { get; set; }
        public Guid AgreementSalesmanId { get; set; }
    }
}
