using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class ExistsChargeCriteria
    {
        public List<string> jobIds { get; set; }
        public List<string> hbls { get; set; }
        public List<string> mbls { get; set; }
    }
}
