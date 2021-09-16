using System.Collections.Generic;

namespace eFMS.API.System.DL.Models.Criteria
{
    public class CatDepartmentCriteria
    {
        public string Type { get; set; }
        public string Keyword { get; set; }
        public List<string> DeptTypes { get; set; }
        public bool? Active { get; set; }
    }
}
