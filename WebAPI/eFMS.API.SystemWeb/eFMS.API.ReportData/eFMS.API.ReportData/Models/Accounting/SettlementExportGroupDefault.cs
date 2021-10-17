﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models.Accounting
{
    public class SettlementExportGroupDefault
    {
        public string JobID { get; set; }
        public string MBL { get; set; }
        public string HBL { get; set; }
        public string CustomNo { get; set; }
        public decimal? SettlementTotalAmount { get; set; }
        public decimal? AdvanceTotalAmount { get; set; }
        public decimal? BalanceTotalAmount { get; set; }
        public decimal? SettlementTotalAmountVND { get; set; }
        public decimal? AdvanceTotalAmountVND { get; set; }
        public decimal? SettlementTotalAmountUSD { get; set; }
        public decimal? AdvanceTotalAmountUSD { get; set; }

        public List<SettlementExportDefault> requestList { get; set; }
    }

    // Class export Accounting Settle List
    public class AccountingSettlementExportGroup
    {
        public string SettlementNo { get; set; }
        public string Requester { get; set; }
        public string Currency { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? DueDate { get; set; }
        public string BankAccountNo { get; set; }
        public string BankAccountName { get; set; }
        public string BankName { get; set; }
        public decimal? SettlementAmount { get; set; }
        public decimal? TotalNetAmount { get; set; }
        public decimal? TotalVatAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAmountVnd { get; set; }
        public decimal? TotalAdvanceAmount { get; set; }
        public string AccountNo { get; set; }
        public List<ShipmentSettlementExportGroup> ShipmentDetail;
    }
}
