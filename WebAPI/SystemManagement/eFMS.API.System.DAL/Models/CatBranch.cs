using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatBranch
    {
        public CatBranch()
        {
            AcctFclsoa = new HashSet<AcctFclsoa>();
            CatDeliveryZoneCode1 = new HashSet<CatDeliveryZoneCode1>();
            PriceCostDestinationBranch = new HashSet<PriceCost>();
            PriceCostOriginBranch = new HashSet<PriceCost>();
            PriceRouteCost = new HashSet<PriceRouteCost>();
        }

        public Guid BranchId { get; set; }
        public int IncreasingId { get; set; }
        public Guid HubId { get; set; }
        public string Address { get; set; }
        public Guid? DistrictId { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public bool? IsVirtualBranch { get; set; }
        public bool? IsHub { get; set; }
        public string PublicNameVn { get; set; }
        public string PublicNameEn { get; set; }
        public string TaxCode { get; set; }
        public string BankAccountVnd { get; set; }
        public string BankAccountUsd { get; set; }
        public string Bank { get; set; }
        public string HotLine { get; set; }
        public string Website { get; set; }
        public string Tel { get; set; }
        public string BillingAddress { get; set; }
        public string BankAddress { get; set; }
        public string SwiftCode { get; set; }
        public Guid? ProvinceId { get; set; }

        public ICollection<AcctFclsoa> AcctFclsoa { get; set; }
        public ICollection<CatDeliveryZoneCode1> CatDeliveryZoneCode1 { get; set; }
        public ICollection<PriceCost> PriceCostDestinationBranch { get; set; }
        public ICollection<PriceCost> PriceCostOriginBranch { get; set; }
        public ICollection<PriceRouteCost> PriceRouteCost { get; set; }
    }
}
