using System;
using System.Collections.Generic;

namespace eFMS.API.ReportData.Models
{
    public partial class CatCommodity
    {
        public string Code { get; set; }
        public string CommodityNameVn { get; set; }
        public string CommodityNameEn { get; set; }
        public bool? Active { get; set; }
    }
}
