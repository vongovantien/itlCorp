using System;

namespace eFMS.API.ReportData.Models
{
    public class CatCommodityGroupCriteria
    {
        public string All { get; set; }
        public string GroupNameVn { get; set; }
        public string GroupNameEn { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
