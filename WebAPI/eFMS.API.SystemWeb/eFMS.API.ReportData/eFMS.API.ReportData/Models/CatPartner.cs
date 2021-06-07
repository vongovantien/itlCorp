using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class CatPartner
    {
        public string AccountNo { get; set; }
        public string FullName { get; set; }
        public string PartnerNameVn { get; set; }
        public string PartnerNameEn { get; set; }
        public string ShortName { get; set; }
        public string AddressVn { get; set; }
        public string AddressEn { get; set; }
        public string AddressShippingEn { get; set; }
        public string TaxCode { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string UserCreatedName { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public string PartnerMode { get; set; }
        public string PartnerLocation { get; set; }
        public string Email { get; set; }
        public string BillingEmail { get; set; }
        public string ContactPerson { get; set; }
        public string BankAccountNo { get; set; }
        public string BankAccountName { get; set; }
        public string BankName { get; set; }
        public string PartnerType { get; set; }
        public string Note { get; set; }
        public bool? Active { get; set; }

    }
}
