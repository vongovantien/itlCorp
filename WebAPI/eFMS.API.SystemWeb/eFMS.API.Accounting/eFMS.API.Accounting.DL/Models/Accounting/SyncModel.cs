using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Accounting
{
    public class SyncModel
    {
        public string Stt { get; set; }
        public string BranchCode { get; set; }
        public string Office { get; set; }
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
        public string AccountNo { get; set; }
        public string ContraAccount { get; set; }
        public string VATAccount { get; set; }
        public string AtchDocNo { get; set; }
        public DateTime? AtchDocDate { get; set; }
        public string AtchDocSerialNo { get; set; }
    }
}
