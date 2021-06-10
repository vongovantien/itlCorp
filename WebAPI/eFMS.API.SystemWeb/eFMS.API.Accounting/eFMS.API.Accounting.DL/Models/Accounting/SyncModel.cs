﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Accounting
{
    public class SyncModel
    {
        public string Stt { get; set; }
        public string BranchCode { get; set; }
        public string OfficeCode { get; set; }
        public string Transcode { get; set; }
        public DateTime? DocDate { get; set; }
        public string ReferenceNo { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMode { get; set; }
        public string LocalBranchCode { get; set; }
        public string CurrencyCode0 { get; set; }
        public decimal ExchangeRate0 { get; set; }
        public string Description0 { get; set; }
        public string DataType { get; set; }
        public string EmailEInvoice { get; set; }
        public List<ChargeSyncModel> Details { get; set; }
    }

    public class ChargeSyncModel
    {
        public string RowId { get; set; }
        public string Ma_SpHt { get; set; }
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string BillEntryNo { get; set; }
        public string MasterBillNo { get; set; }
        public string DeptCode { get; set; }
        public string NganhCode { get; set; }
        public decimal Quantity9 { get; set; }
        public decimal? OriginalUnitPrice { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? OriginalAmount { get; set; }
        public decimal? OriginalAmount3 { get; set; }
        public string OBHPartnerCode { get; set; }
        public string ChargeType { get; set; }      
        public decimal? DueDate { get; set; }
        public string AtchDocNo { get; set; }
        public string AtchDocSerialNo { get; set; }
    }
    
    public class SyncCreditModel
    {
        public string Stt { get; set; }
        public string BranchCode { get; set; }
        public string OfficeCode { get; set; }
        public string Transcode { get; set; }
        public DateTime? DocDate { get; set; }
        public string ReferenceNo { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMode { get; set; }
        public string LocalBranchCode { get; set; }
        public string CurrencyCode { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Description0 { get; set; }
        public string DataType { get; set; }
        public string PaymentMethod { get; set; }
        public List<ChargeCreditSyncModel> Details { get; set; }
    }

    public class ChargeCreditSyncModel
    {
        public string RowId { get; set; }
        public string Ma_SpHt { get; set; }
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string BillEntryNo { get; set; }
        public string MasterBillNo { get; set; }
        public string DeptCode { get; set; }
        public string NganhCode { get; set; }
        public decimal Quantity9 { get; set; }
        public decimal? OriginalUnitPrice { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? OriginalAmount { get; set; }
        public decimal? OriginalAmount3 { get; set; }
        public string OBHPartnerCode { get; set; }
        public string ChargeType { get; set; }
        public string AccountNo { get; set; }
        public string ContraAccount { get; set; }
        public string VATAccount { get; set; }
        public string AtchDocNo { get; set; }
        public DateTime? AtchDocDate { get; set; }
        public string AtchDocSerialNo { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Amount3 { get; set; }
        public string CustomerCodeBook { get; set; }
        public decimal? DueDate { get; set; }
        public string CustomerCodeVAT { get; set; }
        public string CustomerCodeTransfer { get; set; }
        public string AdvanceCustomerCode { get; set; }
        public decimal? RefundAmount { get; set; }
        public string Stt_Cd_Htt { get; set; }
        public int IsRefund { get; set; }
    }
}
