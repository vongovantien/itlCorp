using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class AdjustModel 
    {
        public string CODE { get; set; }
        public string JobNo { get; set; }
        public decimal TotalUSD { get; set; }
        public string PartnerName { get; set; }
        public int TotalShipment { get; set; }
        public int TotalCharge { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal TotalVND { get; set; }
        public string Action { get; set; }
        public string JodId { get; set; }
        public List<AdjustListChargeGrpModel> listChargeGrp { get; set; }
    }

    public class AdjustListChargeGrpModel
    {
        public string JobNo { get; set; }
        public string HBLNo { get; set; }
        public string MBLNo { get; set; }
        public string CustomNo { get; set; }
        public string Pic { get; set; }
        public decimal TotalOrgAmountVND { get; set; }
        public decimal TotalOrgAmountUSD{ get; set; }
        public decimal TotalNetDebit { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalAdjustedVND { get; set; }
        public decimal TotalAdjustedUSD { get; set; }
        public List<AdjustListChargeModel> listCharges { get; set; }
    }

    public class AdjustListChargeModel
    {
        public Guid ID { get; set; }
        public decimal AdjustedVND { get; set; }
        public decimal AdjustedUSD { get; set; }
        public decimal AmountVND { get; set; }
        public decimal VatAmountVND { get; set; }
        public decimal AmountUSD{ get; set; }
        public decimal VatAmountUSD{ get; set; }
        public decimal VatRate { get; set; }
        public decimal OrgNet { get; set; }
        public decimal OrgAmount { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string Currency { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string Note { get; set; }
        public string Type { get; set; }
        public decimal OrgAmountVND { get; set; }

    }
}
