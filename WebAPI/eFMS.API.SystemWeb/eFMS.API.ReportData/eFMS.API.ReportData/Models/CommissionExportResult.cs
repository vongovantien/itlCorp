using System;
using System.Collections.Generic;

namespace eFMS.API.ReportData.Models
{
    public class CommissionExportResult
    {
        public string ForMonth { get; set; }
        public string CustomerName { get; set; }
        public decimal ExchangeRate { get; set; }
        public string BeneficiaryName { get; set; }
        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string TaxCode { get; set; }
        public string PreparedBy { get; set; }
        public string VerifiedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string CrossCheckedBy { get; set; }
        public List<CommissionDetail> Details { get; set; }
    }

    public class CommissionDetail
    {
        public DateTime? ServiceDate { get; set; }
        public string MBLNo { get; set; }
        public string HBLNo { get; set; }
        public string JobId { get; set; }
        public string CustomSheet { get; set; }
        public decimal? ChargeWeight { get; set; }
        public string PortCode { get; set; }
        public decimal BuyingRate { get; set; }
        public decimal SellingRate { get; set; }
        public decimal ComAmount { get; set; }
    }
}
