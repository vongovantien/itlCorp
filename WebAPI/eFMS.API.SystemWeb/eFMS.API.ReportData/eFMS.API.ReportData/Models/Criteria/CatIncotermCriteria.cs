using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models.Criteria
{
    public class CatIncotermCriteria
    {
        public string Code { get; set; }
        public List<string> Service { get; set; }
        public bool? Active { get; set; }
    }
}
