using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class CsShipmentHawbdetail
    {
        public string Hwbno { get; set; }
        public string NoPieces { get; set; }
        public double? GrossWeight { get; set; }
        public double? ChargeWeight { get; set; }
        public double? RateCharge { get; set; }
        public double? Cbm { get; set; }
        public string Unit { get; set; }
        public string Wlbs { get; set; }
        public string RateClass { get; set; }
        public string CommodityItemNo { get; set; }
        public double? Total { get; set; }
        public string NatureQualityOfGoods { get; set; }
        public string Sidescription { get; set; }
        public string MaskNos { get; set; }
    }
}
