using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatCountryCriteria
    {
       
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public SearchCondition condition { get; set; }
    }

  
}
