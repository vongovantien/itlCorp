using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatStageCriteria
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string StageNameVn { get; set; }
        public string StageNameEn { get; set; }
        public int? DepartmentId { get; set; }
        public SearchCondition condition { get; set; }
        public string DepartmentName { get; set; }
        public bool? Active { get; set; }

    }    



}
