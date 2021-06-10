using System;

namespace eFMS.API.ReportData.Models
{
    public partial class CatStage
    {
        public string DeptName { get; set; }
        public string Code { get; set; }
        public string StageNameVn { get; set; }
        public string StageNameEn { get; set; }
        public string DescriptionVn { get; set; }
        public string DescriptionEn { get; set; }
        public bool? Active { get; set; }
    }
}
