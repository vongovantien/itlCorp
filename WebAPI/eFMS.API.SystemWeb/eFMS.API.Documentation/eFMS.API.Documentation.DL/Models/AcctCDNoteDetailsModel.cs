using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class AcctCDNoteDetailsModel
    {
        public string PartnerNameEn { get; set; }
        public string PartnerShippingAddress { get; set; }
        public string PartnerTel { get; set; }
        public string PartnerTaxcode { get; set; }
        public string PartnerFax { get; set; }
        public string PartnerPersonalContact { get; set; }
        public string PartnerId { get; set; }
        public string HbLadingNo { get; set; }
        public string MbLadingNo { get; set; }
        public Guid JobId { get; set; }
        public string JobNo { get; set; }
        public string Pol { get; set; }
        public string PolName { get; set; }
        public string PolCountry { get; set; }
        public string Pod { get; set; }
        public string PodName { get; set; }
        public string PodCountry { get; set; }
        public string Vessel { get; set; }
        public string HbConstainers { get; set; }
        public string HbPackages { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public bool IsLocked { get; set; }
        public decimal? Volum { get; set; }
        public List<CsShipmentSurchargeDetailsModel> ListSurcharges { get; set; }
        public AcctCdnote CDNote { get; set; }
        public decimal? TotalCredit { get; set; }
        public decimal? TotalDebit { get; set; }
        public string ProductService { get; set; }
        public string ServiceMode { get; set; }
        public DateTime? ServiceDate { get; set; }
        public decimal? CBM { get; set; }
        public decimal? GW { get; set; }
        public decimal? SumContainers { get; set; }
        public decimal? SumPackages { get; set; }
        public string WarehouseName { get; set; }
        public string CreatedDate { get; set; }
        public string SoaNo { get; set; }
        public string HbSealNo { get; set; }
        public decimal? HbGrossweight { get; set; }
        public string HbShippers { get; set; }
        public string HbConsignees { get; set; }
        public DateTime? VesselDate { get; set; }
        public decimal? HbChargeWeight { get; set; }
        public string FlexId { get; set; }
        public string Status { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public string SyncStatus { get; set; }
        public string Currency { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string Note { get; set; }
        public string ReasonReject { get; set; }
        public string CreditPayment { get; set; }
        public bool IsExistCurrencyDiffLocal { get; set; }
    }
}
