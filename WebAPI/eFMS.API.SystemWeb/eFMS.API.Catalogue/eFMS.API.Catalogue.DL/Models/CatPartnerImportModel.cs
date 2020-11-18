using eFMS.API.Catalogue.Service.Models;
using System;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPartnerImportModel: CatPartner
    {
        public string DepartmentName { get; set; }
        public string SaleManName { get; set; }
        public string CountryBilling { get; set; }
        public string CityBilling { get; set; }
        public string CountryShipping { get; set; }
        public string CityShipping { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
        public string Profile { get; set; }
        public string PartnerNameEnError { get; set; }
        public string PartnerLocationError { get; set; }
        public string PartnerInternalCodeError { get; set; }
        public string AcReferenceError { get; set; }
        public string PartnerNameVnError { get; set; }
        public string ShortNameError { get; set; }
        public string TaxCodeError { get; set; }
        public string InternalReferenceNoError { get; set; }
        public string PartnerGroupError { get; set; }
        public string ContactPersonError { get; set; }
        public string TelError { get; set; }
        public string AddressEnError { get; set; }
        public string AddressVnError { get; set; }
        public string CityBillingError { get; set; }
        public string CountryBillingError { get; set; }
        public string AddressShippingEnError { get; set; }
        public string AddressShippingVnError { get; set; }
        public string CityShippingError { get; set; }
        public string CountryShippingError { get; set; }
        public string SaleManNameError { get; set; }
        public string ProfileError { get; set; }
        public string ServiceSalemanDefault { get; set; }
        public string ServiceId { get; set; }
        public string ServiceSalemanDefaultError { get; set; }
        public string OfficeSalemanDefault { get; set; }
        public string AcReference { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public string OfficeSalemanDefaultError { get; set; }
        public string PaymentTerm { get; set; }
        public string EffectDate { get; set; }
    }
}
