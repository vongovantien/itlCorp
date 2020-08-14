using eFMS.API.Accounting.DL.Common;
using System;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class AccountReceivableCriteria
    {
        public ARTypeEnum ArType { get; set; }
        public string AcRefId { get; set; }
        public string OverDueDay { get; set; }
        public decimal? DebitRateFrom { get; set; }
        public decimal? DebitRateTo { get; set; }
        public bool AgreementStatus { get; set; }
        public string AgreementExpiredDay { get; set; }
        public string SalesmanId { get; set; }
        public Guid? OfficeId { get; set; }
    }
}
