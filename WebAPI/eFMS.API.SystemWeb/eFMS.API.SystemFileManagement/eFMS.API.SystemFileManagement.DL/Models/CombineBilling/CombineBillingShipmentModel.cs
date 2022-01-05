using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.CombineBilling
{
    public class CombineBillingShipmentModel
    {
        public string Refno { get; set; }
        public Guid? Hblid { get; set; }
        public string JobNo { get; set; }
        public string Mblno { get; set; }
        public string Hwbno { get; set; }
        public string CustomNo { get; set; }
        public decimal? Amount { get; set; }
        public string AmountStr { get; set; }
        public decimal? AmountVnd { get; set; }
        public decimal? AmountUsd { get; set; }
        public string Type { get; set; }
    }
}
