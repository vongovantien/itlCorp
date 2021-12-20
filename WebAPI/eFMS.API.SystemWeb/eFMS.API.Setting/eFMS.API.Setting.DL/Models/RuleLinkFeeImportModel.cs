using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class RuleLinkFeeImportModel
    {
        public Guid? Id { get; set; }
        public string ServiceBuying { get; set; }
        public string ServiceSelling { get; set; }
        public string ChargeBuying { get; set; }
        public string ChargeSelling { get; set; }
        public string PartnerBuying { get; set; }
        public string PartnerSelling { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string RuleName { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
        public string PartnerNameBuying { get; set; }
        public string PartnerNameSelling { get; set; }
        public string ChargeNameBuying { get; set; }
        public string ChargeNameSelling { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
    }
}
