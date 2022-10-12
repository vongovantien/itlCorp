using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatBank
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string BankNameVn { get; set; }
        public string BankNameEn { get; set; }
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
        public Guid? PartnerId { get; set; }
        public string Source { get; set; }
        public Guid? BankId { get; set; }
        public string BankAccountName { get; set; }
    }
}
