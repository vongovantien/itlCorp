using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class GetChargeCriteria
    {
        public string JobId { get; set; }
        public string MBL { get; set; }
        public string HBL { get; set; }
    }
}
