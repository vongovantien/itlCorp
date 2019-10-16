using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class SysOfficeCriteria
    {
        public string All { get; set; }
        public string Code { get; set; }
        public string BranchNameEn { get; set; }
        public string BranchNameVn { get; set; }
        public string ShortName { get; set; }
        public string TaxCode { get; set; }
        public bool? Active { get; set; }
        public Guid Buid { get; set; }
        public string CompanyName { get; set; }
    }
}
