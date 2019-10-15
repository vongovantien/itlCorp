using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class SysOfficeModel
    {
        public int No { get; set; }
        public string Code { get; set; }
        public string BranchNameEn { get; set; }
        public string BranchNameVn { get; set; }
        public string ShortName { get; set; }
        public string AddressVn { get; set; }
        public string Taxcode { get; set; }
        public string CompanyName { get; set; }
        public bool? Active { get; set; }

    }
}
