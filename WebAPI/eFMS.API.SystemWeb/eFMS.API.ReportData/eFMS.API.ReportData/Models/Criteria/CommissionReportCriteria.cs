using eFMS.API.ReportData.Models.Criteria;

namespace eFMS.API.ReportData.Models.Criteria
{
    public class CommissionReportCriteria : GeneralReportCriteria
    {
        public string CustomNo { get; set; }
        public string Beneficiary { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
