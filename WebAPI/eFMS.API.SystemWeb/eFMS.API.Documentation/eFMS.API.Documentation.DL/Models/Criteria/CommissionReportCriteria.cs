using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class CommissionReportCriteria : GeneralReportCriteria
    {
        public string CustomNo { get; set; }
        public string Beneficiary { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
