using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.Service.Models
{
    public partial class CatPartner
    {
        public string Id { get; set; }
        public string PartnerGroup { get; set; }
        public string PartnerNameVn { get; set; }
        public string PartnerNameEn { get; set; }
        public string ContactPerson { get; set; }
        public string AddressVn { get; set; }
        public string AddressEn { get; set; }
        public string AddressShippingVn { get; set; }
        public string AddressShippingEn { get; set; }
        public string ShortName { get; set; }
        public int? DepartmentId { get; set; }
        public short? CountryId { get; set; }
        public short? CountryShippingId { get; set; }
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
        public Guid? ProvinceShippingId { get; set; }
        public string ParentId { get; set; }
        public decimal? PercentCredit { get; set; }
        public bool? AlertPercentCreditEmail { get; set; }
        public string PaymentBeneficiary { get; set; }
        public bool? UsingParrentRateCard { get; set; }
        public string SugarId { get; set; }
        public int? BookingOverdueDay { get; set; }
        public bool? FixRevenueByProject { get; set; }
        public string ZipCode { get; set; }
        public string ZipCodeShipping { get; set; }
        public string SwiftCode { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string InternalReferenceNo { get; set; }
        public string CoLoaderCode { get; set; }
        public string WorkPhoneEx { get; set; }
        public short? GroupId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public string RoundUpMethod { get; set; }
        public string ApplyDim { get; set; }
        public string BillingPhone { get; set; }
        public string BillingEmail { get; set; }
        public string PartnerType { get; set; }
        public string PartnerMode { get; set; }
        public string PartnerLocation { get; set; }
    }
}
