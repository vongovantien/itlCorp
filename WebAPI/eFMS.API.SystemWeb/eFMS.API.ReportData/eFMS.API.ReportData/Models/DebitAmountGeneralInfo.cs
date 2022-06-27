using System;

namespace eFMS.API.ReportData.Models
{
    public class DebitAmountGeneralInfo
    {
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string ContracType { get; set; }
        public string ContractNo { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string Currency { get; set; }
    }
}
