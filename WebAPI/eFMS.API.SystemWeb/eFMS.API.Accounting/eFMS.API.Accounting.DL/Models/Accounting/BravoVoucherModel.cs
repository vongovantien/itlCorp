using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Accounting
{
    public class BravoVoucherModel
    {
        public string DataType { get => "VOUCHER"; set => DataType = value; }
        public Guid Stt { get; set; }
        public string AccountNo { get; set; }
        public string BranchCode { get; set; }
        public string LocalBranchCode { get; set; }
        public string OfficeCode { get; set; }
        public string TransCode { get; set; }
        public string ReferenceNo { get; set; }
        public string CustomerCode { get; set; }  // Đối tượng voucher
        public string CustomerName { get; set; }
        public string CustomerMode { get; set; }
        public DateTime? DocDate { get; set; }  // Ngày tạo voucher
        public string Description0 { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? ExchangeRate { get; set; }  // Tỷ giá ngày tạo voucher
        public string PaymentMethod { get; set; }
        public List<BravoVoucherChargeModel> Details { get; set; }
    }

    public class BravoVoucherChargeModel
    {
        public string NganhCode { get => "FWD"; set => NganhCode = value; }
        public Guid RowId { get; set; } // Charge ID
        public string Ma_SpHt { get; set; } // Job No
        public string ItemCode { get; set; } // Charge Code
        public string Description { get; set; }
        public string Unit { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string BillEntryNo { get; set; }
        public string MasterBillNo { get; set; }
        public string DeptCode { get; set; }
        public decimal Quantity9 { get; set; } // Qty
        public decimal? OriginalUnitPrice { get; set; } // UnitPrice
        public decimal? TaxRate { get; set; }   // VAT
        public decimal? OriginalAmount { get; set; } // Amount
        public decimal? OriginalAmount3 { get; set; } // VAT Amount
        public string OBHPartnerCode { get; set; }
        public string AccountNo { get; set; }
        public string ContracAccount { get; set; }
        public string VATAccount { get; set; }
        public string AtchDocNo { get; set; } // Invoice
        public DateTime? AtchDocDate { get; set; } // Invoice Date
        public string AtchDocSerieNo { get; set; } // Serie
        public string ChargeType { get; set; } // CREDIT / OBH
        public decimal? Amount { get; set; }
        public decimal? Amount3 { get; set; }







    }
}
