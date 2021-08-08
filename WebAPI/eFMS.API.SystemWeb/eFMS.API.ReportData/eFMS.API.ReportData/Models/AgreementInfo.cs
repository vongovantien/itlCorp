using System;

namespace eFMS.API.ReportData.Models
{
    public class AgreementInfo
    {
        public string PartnerCode { get; set; }
        public string PartnerNameEn { get; set; }
        public string PartnerNameVn { get; set; }
        public string AgreementType { get; set; }
        public string AgreementNo { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? PaymentTerm { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string Currency { get; set; }
        public string SaleManName { get; set; }
        public bool? ARComfirm { get; set; }
        public bool? Active { get; set; }
        public string Service { get; set; }
        public string Office { get; set; }
        public string UserCreatedName { get; set; }
    }
}
