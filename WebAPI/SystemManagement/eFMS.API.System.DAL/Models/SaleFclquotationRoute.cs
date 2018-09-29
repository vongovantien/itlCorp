using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SaleFclquotationRoute
    {
        public Guid Id { get; set; }
        public string RoadId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid PlaceFrom { get; set; }
        public Guid PlaceTo { get; set; }
        public string PoReceipt { get; set; }
        public string PoTransfer { get; set; }
        public string PoDelivery { get; set; }
        public int? CommodityId { get; set; }
        public string ContainerTypeId { get; set; }
        public short? VolumeId { get; set; }
        public int? WeightRangeId { get; set; }
        public decimal? RealWeight { get; set; }
        public short? ServiceType { get; set; }
        public int? LenthKm { get; set; }
        public int? DaysFrom { get; set; }
        public int? DaysTo { get; set; }
        public int? Trip { get; set; }
        public decimal? Kratio { get; set; }
        public decimal? BuyingPrice { get; set; }
        public decimal? MiniumCostAmount { get; set; }
        public decimal? FuelCostAmount { get; set; }
        public decimal? MnrcostAmount { get; set; }
        public decimal? FixedCostAmount { get; set; }
        public decimal? OverheadCostAmount { get; set; }
        public decimal? ChargeCostAmount { get; set; }
        public decimal? PriceSaleRequest { get; set; }
        public decimal? PriceBack { get; set; }
        public decimal? OtherRevenueAmount { get; set; }
        public decimal? PriceCustomer { get; set; }
        public decimal? PriceMarket { get; set; }
        public decimal? FinalPrice { get; set; }
        public string CurrencyId { get; set; }
        public string Type { get; set; }
        public string QuotationRouteType { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public string SaleMember { get; set; }
        public string SaleResource { get; set; }
        public bool? IsMergedQuotation { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public string JourneyNote { get; set; }
        public string OpsManagerId { get; set; }
        public string OpsManagerStatus { get; set; }
        public DateTime? OpsManagerDate { get; set; }
        public string OpsManagerNote { get; set; }
        public string OpsapprovedMethod { get; set; }
        public string AccountId { get; set; }
        public string AccountStatus { get; set; }
        public DateTime? AccountDate { get; set; }
        public string AccountNote { get; set; }
        public string AccountantApprovedMethod { get; set; }
        public string SaleManagerId { get; set; }
        public string SaleManagerStatus { get; set; }
        public DateTime? SaleManagerDate { get; set; }
        public string SaleManagerNote { get; set; }
        public string SaleManApprovedMethod { get; set; }
        public Guid? QuotationRouteDuplicatedId { get; set; }
        public decimal? FuelConsumption { get; set; }
        public decimal? PriceSaleRequestVat { get; set; }
        public decimal? PriceBackVat { get; set; }
        public string SellingCurrencyId { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public decimal? ReferCustomerFee { get; set; }
    }
}
