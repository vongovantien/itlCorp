using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.Accounting
{
    public class BravoSettlementModel
    {
        public Guid Stt { get; set; }
        public string DataType { get => "SETTLEMENT"; set => DataType = value; }
        public string TransCode { get; set; }
        public string BranchCode { get; set; }
        public string OfficeCode { get; set; }
        public DateTime? DocDate { get; set; }
        public string ReferenceNo { get; set; }
        public string CustomerMode { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CurrencyCode { get; set; } = "VND";
        public decimal ExchangeRate { get; set; }
        public string Description0 { get; set; }
        public string LocalBranchCode { get; set; }
        public string PaymentMethod { get; set; }
        public string Payee { get; set; }
        public decimal? SettleAmount { get; set; }
        public List<BravoSettlementRequestModel> Details { get; set; }

    }

    public class BravoSettlementRequestModel
    {
        public string NganhCode { get => "FWD"; set => NganhCode = value; }
        public string RowId { get; set; } // Charge ID
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
        public string AtchDocSerialNo { get; set; } // Serie
        public string ChargeType { get; set; } // CREDIT / OBH
        public decimal? Amount { get; set; }
        public decimal? Amount3 { get; set; }
        public string CustomerCodeBook { get; set; } // Mã đối tượng Payer,
        public decimal? DueDate { get; set; }
        public string CustomerCodeVAT { get; set; }
        public string CustomerCodeTransfer { get; set; }
        public string AdvanceCustomerCode { get; set; } // Mã Đối tượng tạm ứng
        public decimal? RefundAmount { get; set; } // Tổng Số tiền hoàn ứng
        public string Stt_Cd_Htt { get; set; } // Số Ref Advance
        public int IsRefund { get; set; } // 0. Nghiệp vụ chi phí normal, 1.Nghiệp vụ chi phí Có hoàn ứng.

        // Custom
        public string AdvanceNo { get; set; }
        public Guid HblId { get; set; }
        public string ClearanceNo { get; set; }

        // Amount
        public decimal? NetAmountVND { get; set; }
        public decimal? NetAmountUSD { get; set; }
        public decimal? VatAmountVND { get; set; }
        public decimal? VatAmountUSD { get; set; }
    }
}
