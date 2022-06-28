using System;
namespace eFMS.API.Accounting.Service.ViewModels
{
    public class sp_GetBillingWithSalesman
    {
        public string PartnerId { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public string PaymentStatus { get; set; }
        public Guid OfficeId { get; set; }
        public string Service { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAmountVND { get; set; }
        public decimal? TotalAmountUSD { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountVND { get; set; }
        public decimal? PaidAmountUSD { get; set; }
        public decimal? UnpaidAmount { get; set; }
        public decimal? UnpaidAmountVND { get; set; }
        public decimal? UnpaidAmountUSD { get; set; }
        public string Salesman { get; set; }
        public string SalesmanId { get; set; }
        public string Code { get; set; }
        public string InvoiceNo { get; set; }
        public string BillingNo { get; set; }
        public string Type { get; set; }
        public int OverdueDays { get; set; }
    }

    public class GetArBBillingWithSalesman
    {
        public string PartnerId { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public string PaymentStatus { get; set; }
        public Guid OfficeId { get; set; }
        public string Service { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAmountVND { get; set; }
        public decimal? TotalAmountUSD { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountVND { get; set; }
        public decimal? PaidAmountUSD { get; set; }
        public decimal? UnpaidAmount { get; set; }
        public decimal? UnpaidAmountVND { get; set; }
        public decimal? UnpaidAmountUSD { get; set; }
        public string Salesman { get; set; }
        public string SalesmanId { get; set; }
        public string Code { get; set; }
        public string InvoiceNo { get; set; }
        public string BillingNo { get; set; }
        public string Type { get; set; }
        public int OverdueDays { get; set; }
    }
}
