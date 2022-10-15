﻿namespace eFMS.API.Report.DL.Models
{
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
