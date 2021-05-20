using System;

namespace eFMS.API.ReportData.Models
{
    public partial class CatCommodityGroup
    {
        public short Id { get; set; }
        public string GroupNameVn { get; set; }
        public string GroupNameEn { get; set; }
        public bool? Active { get; set; }
    }
}
