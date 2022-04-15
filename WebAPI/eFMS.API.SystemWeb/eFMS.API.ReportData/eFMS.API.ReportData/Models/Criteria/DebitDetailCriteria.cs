using System;

namespace eFMS.API.ReportData.Models.Criteria
{
    public class DebitDetailCriteria
    {
        public Guid argeementId { get; set; }
        public string option { get; set; }
        public string officeId { get; set; }
        public string serviceCode { get; set; }
        public int overDueDay { get; set; } = 0;
    }
}
