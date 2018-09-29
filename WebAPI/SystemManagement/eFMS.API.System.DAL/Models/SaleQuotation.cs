using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SaleQuotation
    {
        public SaleQuotation()
        {
            SaleQuotationRoute = new HashSet<SaleQuotationRoute>();
        }

        public Guid Id { get; set; }
        public Guid WorkPlaceId { get; set; }
        public string Code { get; set; }
        public string CustomerId { get; set; }
        public int? ContractId { get; set; }
        public string Tel { get; set; }
        public string QuotationScope { get; set; }
        public int RateCardId { get; set; }
        public string SaleMember { get; set; }
        public string SaleResource { get; set; }
        public int? MaximumDelayTime { get; set; }
        public string MaximumDelayTimeUnit { get; set; }
        public int? PaymentDeadline { get; set; }
        public string PaymentDeadlineUnit { get; set; }
        public string ChiefAccountantId { get; set; }
        public string ChiefAccountantStatus { get; set; }
        public DateTime? ChiefAccountantDate { get; set; }
        public string ChiefAccountantNote { get; set; }
        public string ChiefAccountantApprovedMethod { get; set; }
        public string SaleManagerId { get; set; }
        public string SaleManagerStatus { get; set; }
        public DateTime? SaleManagerDate { get; set; }
        public string SaleManagerNote { get; set; }
        public string SaleManApprovedMethod { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public PriceRateCard RateCard { get; set; }
        public ICollection<SaleQuotationRoute> SaleQuotationRoute { get; set; }
    }
}
