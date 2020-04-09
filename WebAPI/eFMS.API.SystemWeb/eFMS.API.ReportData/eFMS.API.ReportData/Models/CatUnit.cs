using System;
using System.Collections.Generic;

namespace eFMS.API.ReportData.Models
{
    public partial class CatUnit
    {
        public string Code { get; set; }
        public string UnitNameVn { get; set; }
        public string UnitNameEn { get; set; }
        public string UnitType { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionVn { get; set; }
        public bool? Active { get; set; }
    }
}
