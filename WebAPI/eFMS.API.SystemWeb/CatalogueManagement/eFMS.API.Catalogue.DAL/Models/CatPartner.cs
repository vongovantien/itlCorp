using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatPartner
    {
        public string Id { get; set; }
        public string PartnerGroup { get; set; }
        public string PartnerNameVn { get; set; }
        public string PartnerNameEn { get; set; }
        public string AddressVn { get; set; }
        public string AddressEn { get; set; }
        public string ShortName { get; set; }
        public short? CountryId { get; set; }
        public string AccountNo { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string TaxCode { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string BankAccountNo { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountAddress { get; set; }
        public string Note { get; set; }
        public string SalePersonId { get; set; }
        public bool? Public { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public bool? RefuseEmail { get; set; }
        public bool? ReceiveAttachedWaybill { get; set; }
        public string RoundedSoamethod { get; set; }
        public bool? TaxExemption { get; set; }
        public bool? ReceiveEtaemail { get; set; }
        public bool? ShowInDashboard { get; set; }
        public Guid? ProvinceId { get; set; }
        public string ParentId { get; set; }
        public decimal? PercentCredit { get; set; }
        public bool? AlertPercentCreditEmail { get; set; }
        public string PaymentBeneficiary { get; set; }
        public bool? UsingParrentRateCard { get; set; }
        public string SugarId { get; set; }
        public int? BookingOverdueDay { get; set; }
        public bool? FixRevenueByProject { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
