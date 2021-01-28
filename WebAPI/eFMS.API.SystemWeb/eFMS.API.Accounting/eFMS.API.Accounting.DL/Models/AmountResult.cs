using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class AmountResult
    {
        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
