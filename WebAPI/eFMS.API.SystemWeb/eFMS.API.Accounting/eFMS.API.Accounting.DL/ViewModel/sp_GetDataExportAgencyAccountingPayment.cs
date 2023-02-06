using eFMS.API.Accounting.DL.Models.AccountingPayment;
using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.ViewModel
{
    public class sp_GetDataExportAgencyAccountingPayment
    {
        public string RefId { get; set; }
        public string BillingRefNo { get; set; }
        public string InvoiceNoReal { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string PartnerCode { get; set; }
        public string ParentCode { get; set; }
        public string Currency { get; set; }
        public DateTime? IssuedDate { get; set; }
        public decimal? PaidAmountVnd { get; set; }
        public decimal? PaidAmountUsd { get; set; }
        public DateTime? DueDate { get; set; }
        public int OverdueDays { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string VoucherId { get; set; }
        public decimal? PaymentTerm { get; set; }
        public Guid? ReceiptId { get; set; }
        public DateTime? ConfirmBillingDate { get; set; }
        public Guid? OfficeId { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal? UnpaidAmountVnd { get; set; }
        public decimal? UnpaidAmountUsd { get; set; }
        public decimal? DebitTotalAmountVnd { get; set; }
        public decimal? DebitTotalAmountUsd { get; set; }
        public string AccountNo { get; set; }
        public string PaymentStatus { get; set; }
        public string SourceModified { get; set; }
        public string BillingRefNoType { get; set; }
        public string JobNo { get; set; }
        public string Mblno { get; set; }
        public string Hblno { get; set; }
        public Guid? Hblid { get; set; }
        public string PaymentNo { get; set; }
        public string CurrencyId { get; set; }
        public string PaymentType { get; set; }
        public string UserCreated { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? CompanyId { get; set; }
        public string PaymentMethod { get; set; }
        public string Note { get; set; }
        public decimal? PaymentAmountVnd { get; set; }
        public decimal? PaymentAmountUsd { get; set; }
        public decimal? UnpaidPaymentAmountUsd { get; set; }
        public decimal? UnpaidPaymentAmountVnd { get; set; }
        public string InvoiceNo { get; set; }
        public decimal? CusAdvanceAmountVnd { get; set; }
        public decimal? CusAdvanceAmountUsd { get; set; }
        public DateTime? PaymentDatetimeCreated { get; set; }
        public string PaymentRefNo { get; set; }
        public DateTime? PaymentDate { get; set; }
        public Guid? AgreementId { get; set; }
    }
}
