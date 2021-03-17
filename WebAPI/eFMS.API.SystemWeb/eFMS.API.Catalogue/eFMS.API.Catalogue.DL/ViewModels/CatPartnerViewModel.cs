using eFMS.API.Catalogue.Service.Models;
using System;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatPartnerViewModel 
    {
        public string Id { get; set; }
        public string PartnerGroup { get; set; }
        public string PartnerNameVn { get; set; }
        public string PartnerNameEn { get; set; }
        public string ShortName { get; set; }
        public string TaxCode { get; set; }
        public string SalePersonId { get; set; }
        public string Tel { get; set; }
        public string AddressEn { get; set; }
        public string AddressVn { get; set; }
        public string Fax { get; set; }
        public string CoLoaderCode { get; set; }
        public string RoundUpMethod { get; set; }
        public string ApplyDim { get; set; }
        public string AccountNo { get; set; }
        public DateTime? InActiveOn { get; set; }
        public string UserCreatedName { get; set; }
        public string SalePersonName { get; set; }
        public string UserCreated { get; set; }
        public short? GroupId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public string PartnerType { get; set; }
        public string CountryShippingName { get; set; }
        public string TaxCodeAbbrName { get; set; }
        public string BankAccountNo { get; set; }
        public string BankAccountName { get; set; }
    }
}
