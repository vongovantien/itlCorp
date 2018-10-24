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
        public Condition condition { get; set; }
        //public string DescriptionVn { get; set; }
        //public string DescriptionEn { get; set; }
        //public string UserCreated { get; set; }
        //public DateTime? DatetimeCreated { get; set; }
        //public string UserModified { get; set; }
        //public DateTime? DatetimeModified { get; set; }
        //public bool? Inactive { get; set; }
        //public DateTime? InactiveOn { get; set; }
     
    }
    public enum Condition
    {
        AND,
        OR
    }


}
