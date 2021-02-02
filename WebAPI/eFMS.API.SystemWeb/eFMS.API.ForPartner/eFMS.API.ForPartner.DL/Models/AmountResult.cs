namespace eFMS.API.ForPartner.DL.Models
{
    public class AmountResult
    {
        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal ExchangeRate { get; set; }
    }

    public class AmountSurchargeResult
    {
        public decimal NetAmountOrig { get; set; } //Tiền trước thuế (Original)
        public decimal VatAmountOrig { get; set; } //Tiền thuế (Original)
        public decimal GrossAmountOrig { get; set; } //Tiền sau thuế (Original)
        public decimal FinalExchangeRate { get; set; }
        public decimal AmountVnd { get; set; }
        public decimal VatAmountVnd { get; set; }
        public decimal AmountUsd { get; set; }
        public decimal VatAmountUsd { get; set; }
    }
}
