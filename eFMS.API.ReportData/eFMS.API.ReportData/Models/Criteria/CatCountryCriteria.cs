using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.ReportData.Models
{
    public class CatCountryCriteria
    {
       
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public bool? Inactive { get; set; }
        public SearchCondition condition { get; set; }
    }
    public enum SearchCondition
    {
        AND = 0,
        OR = 1
    }


}
