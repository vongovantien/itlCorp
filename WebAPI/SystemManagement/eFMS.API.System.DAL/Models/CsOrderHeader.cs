using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsOrderHeader
    {
        public CsOrderHeader()
        {
            CsOrderDetail = new HashSet<CsOrderDetail>();
        }

        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string OrderCode { get; set; }
        public string Mawb { get; set; }
        public string CustomerId { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public int ServiceTypeMappingId { get; set; }
        public string RoadId { get; set; }
        public Guid? OriginBranchId { get; set; }
        public Guid? OriginHubId { get; set; }
        public short? CurrentStatusId { get; set; }
        public string RefNo { get; set; }
        public int? CustomerCreditTerm { get; set; }
        public decimal? CargoValue { get; set; }
        public string SalePersonId { get; set; }
        public short? UnitId { get; set; }
        public short? ChargeTypeId { get; set; }
        public string InvoiceAddress { get; set; }
        public short? InvoiceDistrictId { get; set; }
        public short? InvoiceCountryId { get; set; }
        public string InvoicePostalCode { get; set; }
        public bool? BillingGenerated { get; set; }
        public string BillingNo { get; set; }
        public decimal? TotalExcludeSurcharge { get; set; }
        public decimal? TotalSurcharge { get; set; }
        public decimal? TotalExcludeVat { get; set; }
        public int? Vatrate { get; set; }
        public decimal? Vat { get; set; }
        public decimal? TotaIncludeVat { get; set; }
        public decimal? TotalCost { get; set; }
        public string Remarks { get; set; }
        public bool? Vatinclude { get; set; }
        public bool? TreatAsAbandoned { get; set; }
        public byte[] Version { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string ContactPersonId { get; set; }
        public Guid? InvoiceProvinceId { get; set; }
        public string CurrencyId { get; set; }
        public string CustomerDebitId { get; set; }

        public ICollection<CsOrderDetail> CsOrderDetail { get; set; }
    }
}
