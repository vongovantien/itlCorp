using System;

namespace eFMS.API.Accounting.DL.ViewModel
{
    public class ChargeSoaUpdateTable
    {
        public Guid? Id { get; set; }
        public string Soano { get; set; }
        public string PaySoano { get; set; }
        public DateTime? ExchangeDate { get; set; }
        public decimal? FinalExchangeRate { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? Total { get; set; }
        public decimal? AmountVnd { get; set; }
        public decimal? VatAmountVnd { get; set; }
        public decimal? AmountUsd { get; set; }
        public decimal? VatAmountUsd { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string UserModified { get; set; }
    }
}
