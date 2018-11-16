using System;
using System.Collections.Generic;

namespace eFMS.IdentityServer.Service.Models
{
    public partial class CatPartnerContract
    {
        public int Id { get; set; }
        public string PartnerId { get; set; }
        public string ContractNo { get; set; }
        public DateTime? EffectiveOn { get; set; }
        public DateTime? ExpiryOn { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public string ActiveBy { get; set; }
        public DateTime? ActiveOn { get; set; }
        public decimal? CreditAmount { get; set; }
        public short? PaymentDeadline { get; set; }
        public string PaymentDeadlineUnit { get; set; }
        public short? NumberDayContractExpiration { get; set; }
        public bool? AlertNumberDayContractExpiration { get; set; }
        public string SugarId { get; set; }
        public bool? IsTrial { get; set; }
    }
}
