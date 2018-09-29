using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceRateCard
    {
        public PriceRateCard()
        {
            PriceRateCardCondition = new HashSet<PriceRateCardCondition>();
            SaleQuotation = new HashSet<SaleQuotation>();
        }

        public int Id { get; set; }
        public Guid BranchId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Calculation { get; set; }
        public string Type { get; set; }
        public bool? ConsolidatedAtHub { get; set; }
        public string SaleNote { get; set; }
        public DateTime? EffectiveOn { get; set; }
        public DateTime? ExpiryOn { get; set; }
        public string OpsmanId { get; set; }
        public DateTime? OpsmanApprovedOn { get; set; }
        public string OpsmanStatus { get; set; }
        public string OpsmanNote { get; set; }
        public string ChiefAccountantId { get; set; }
        public DateTime? ChiefAccountantApprovedOn { get; set; }
        public string ChiefAccountantStatus { get; set; }
        public string ChiefAccountantNote { get; set; }
        public string SaleManId { get; set; }
        public DateTime? SaleManApprovedOn { get; set; }
        public string SaleManStatus { get; set; }
        public string SaleManNote { get; set; }
        public string HeadBuid { get; set; }
        public DateTime? HeadBuapprovedOn { get; set; }
        public string HeadBustatus { get; set; }
        public string HeadBunote { get; set; }
        public string UserRevokeId { get; set; }
        public DateTime? UserRevokeOn { get; set; }
        public string UserRevokeNote { get; set; }
        public short? Status { get; set; }
        public string QuotationType { get; set; }
        public string GettingPriceMethod { get; set; }
        public string QuotedRouteType { get; set; }
        public string CurrencyId { get; set; }
        public decimal? FuelPrice { get; set; }
        public string ChiefAccountantApprovedMethod { get; set; }
        public string SaleManApprovedMethod { get; set; }
        public string HeadBuapprovedMethod { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string ReferCustomerId { get; set; }
        public decimal? ReferCustomerPercent { get; set; }

        public ICollection<PriceRateCardCondition> PriceRateCardCondition { get; set; }
        public ICollection<SaleQuotation> SaleQuotation { get; set; }
    }
}
