using System;

namespace eFMS.API.ReportData.Models
{
    public class CatPartnerCriteria
    {
        public string All { get; set; }
        public CatPartnerGroupEnum PartnerGroup { get; set; }
        public string Id { get; set; }
        public string PartnerNameVn { get; set; }
        public string PartnerNameEn { get; set; }
        public string ShortName { get; set; }
        public string AddressVn { get; set; }
        public string AddressEn { get; set; }
        public string TaxCode { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public bool? Active { get; set; }
        public bool? AgreeActive { get; set; }
        public string AccountNo { get; set; }
        public string Author { get; set; }
        public string PartnerType { get; set; }
        public string Saleman { get; set; }
        public DateTime? DatetimeCreatedFrom { get; set; }
        public DateTime? DatetimeCreatedTo { get; set; }
    }
    public enum CatPartnerGroupEnum
    {
        AGENT = 1,
        CONSIGNEE = 2,
        CUSTOMER = 3,
        PAYMENTOBJECT = 4,
        PETROLSTATION = 5,
        SHIPPER = 6,
        SHIPPINGLINE = 7,
        SUPPLIER = 8,
        SUPPLIERMATERIAL = 9,
        CARRIER = 10,
        AIRSHIPSUP = 11
    }
}
