using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class HousbillProfit
    {
        public Guid HBLID { get; set; }
        public string HBLNo { get; set; }
        public HouseBillTotalCharge HouseBillTotalCharge { get; set; }
        public decimal ProfitLocal { get; set; }
        public decimal ProfitUSD { get; set; }
    }
    public class HouseBillTotalCharge
    {
        public decimal TotalBuyingUSD { get; set; }
        public decimal TotalSellingUSD { get; set; }
        public decimal TotalOBHUSD { get; set; }
        public decimal TotalBuyingLocal { get; set; }
        public decimal TotalSellingLocal { get; set; }
        public decimal TotalOBHLocal { get; set; }
    }
}
