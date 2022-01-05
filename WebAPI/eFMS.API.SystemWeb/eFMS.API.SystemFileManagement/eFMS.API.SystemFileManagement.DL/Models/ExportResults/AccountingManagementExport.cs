﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.ExportResults
{
    public class AccountingManagementExport
    {
        public Guid SurchargeId { get; set; }
        public Guid ChargeId { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string JobNo { get; set; }
        public string Hbl { get; set; }
        public string ContraAccount { get; set; }
        public decimal? OrgAmount { get; set; }
        public decimal? Vat { get; set; }
        public decimal? OrgVatAmount { get; set; }
        public string VatAccount { get; set; }
        public string Currency { get; set; }
        public DateTime? ExchangeDate { get; set; }
        public decimal? FinalExchangeRate { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? AmountVnd { get; set; }
        public decimal? VatAmountVnd { get; set; }
        public string VatPartnerId { get; set; }
        public string VatPartnerCode { get; set; }
        public string VatPartnerName { get; set; } //Short Name Partner
        public string VatPartnerAddress { get; set; }
        public string ObhPartnerCode { get; set; }
        public string ObhPartner { get; set; }
        public string InvoiceNo { get; set; }
        public string Serie { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string CdNoteNo { get; set; }
        public decimal? Qty { get; set; }
        public string UnitName { get; set; }
        public decimal? UnitPrice { get; set; }
        public string Mbl { get; set; }
        public string SoaNo { get; set; }
        public string SettlementCode { get; set; }
        public Guid? AcctManagementId { get; set; }
        public DateTime? Date { get; set; } //Date trên VAT Invoice Or Voucher 
        public string VoucherId { get; set; } //VoucherId trên VAT Invoice Or Voucher
        public string PartnerId { get; set; } //Partner trên VAT Invoice Or Voucher
        public string AccountNo { get; set; } //Account No trên VAT Invoice Or Voucher
        public string VatPartnerNameEn { get; set; } //Partner Name En của Charge
        public string VatPartnerNameVn { get; set; } //Partner Name Local của Charge
        public string Description { get; set; } //Description trên VAT Invoice Or Voucher
        public bool IsTick { get; set; } //Đánh dấu
        public decimal PaymentTerm { get; set; } //Thời hạn thanh toán
        public string DepartmentCode { get; set; } //Mã bộ phận
        public string CustomNo { get; set; }
        public string PaymentMethod { get; set; } //Hình thức thanh toán trên VAT Invoice Or Voucher
        public string StatusInvoice { get; set; } //Tình trạng hóa đơn
        public string VatPartnerEmail { get; set; } //Email Partner của charge
        public DateTime? ReleaseDateEInvoice { get; set; } //Ngày phát hành E-Invoice
        public string TkNoVat { get; set; } //Tài khoản Nợ VAT
        public string TkCoVat { get; set; } //Tài khoản Có VAT

    }
}
