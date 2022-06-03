using System;
namespace eFMS.API.Accounting.Service.ViewModels
{
    public class sp_GetBillingWithSalesman
    {
        public Guid ID { get; set; }
        public string PartnerId { get; set; }
        public Guid OfficeId { get; set; }
        public string Service { get; set; }
        public DateTime DatetimeCreated { get; set; }
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
    }
}
