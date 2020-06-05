using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.ExportResults
{
    public class AccountingManagementExport : ChargeOfAccountingManagementModel
    {
        public DateTime? Date { get; set; } //Date trên VAT Invoice Or Voucher 
        public string VoucherId { get; set; } //VoucherId trên VAT Invoice Or Voucher
        public string PartnerId { get; set; } //Partner trên VAT Invoice Or Voucher
        public string AccountNo { get; set; } //Account No trên VAT Invoice Or Voucher
        public string VatPartnerNameEn { get; set; } //Partner Name En của Charge
        public string Description { get; set; } //Description trên VAT Invoice Or Voucher
        public bool IsTick { get; set; } //Đánh dấu
        public decimal PaymentTerm { get; set; } //Thời hạn thanh toán
        public string DepartmentCode { get; set; } //Mã bộ phận
        public string CustomNo { get; set; }
        public string PaymentMethod { get; set; } //Hình thức thanh toán trên VAT Invoice Or Voucher
        public string StatusInvoice { get; set; } //Tình trạng hóa đơn
        public string VatPartnerEmail { get; set; } //Email Partner của charge
        public DateTime? ReleaseDateEInvoice { get; set; } //Ngày phát hành E-Invoice

    }
}
