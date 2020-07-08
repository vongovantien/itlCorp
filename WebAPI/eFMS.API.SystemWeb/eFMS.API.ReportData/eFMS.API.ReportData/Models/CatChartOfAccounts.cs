using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class CatChartOfAccounts
    {
        public string AccountCode { get; set; }
        public string AccountNameEn { get; set; }
        public string AccountNameLocal { get; set; }
        public bool? Active { get; set; }
    }
}
