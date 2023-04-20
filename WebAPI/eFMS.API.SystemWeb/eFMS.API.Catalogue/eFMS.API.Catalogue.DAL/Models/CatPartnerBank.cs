using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatPartnerBank
    {
        public Guid Id { get; set; }
        public Guid? BankId { get; set; }
        public Guid? PartnerId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string SwiftCode { get; set; }
        public string BankAddress { get; set; }
        public string Note { get; set; }
        public string BankAccountNo { get; set; }
        public string Source { get; set; }
        public string BankAccountName { get; set; }
        public string ApproveStatus { get; set; }
        public string BeneficiaryAddress { get; set; }
        public string ApproveDescription { get; set; }
    }
}
