using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatChartOfAccountsCriteria
    {
        public string All { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameLocal { get; set; }
        public string AccountNameEn { get; set; }
        public string Active { get; set; }
    }
}
