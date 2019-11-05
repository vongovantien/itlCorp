using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class HousbillProfit
    {
        public decimal TotalBuyingUSD { get; set; }
        public decimal TotalSellingUSD { get; set; }
        public decimal TotalOBHUSD { get; set; }
        public decimal TotalBuyingLocal { get; set; }
        public decimal TotalSellingLocal { get; set; }
        public decimal TotalOBHLocal { get; set; }
    }
}
