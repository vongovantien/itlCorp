using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models.Criteria
{
    public class CatDepartmentGroupCriteria
    {
        public string UserId { get; set; }
        public int? DepartmentId { get; set; }
        public int? GroupId { get; set; }
        public string DepartmentName { get; set; }
        public string GroupName { get; set; }
        public string Type { get; set; }

    }
}
