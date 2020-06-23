using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models.Criteria
{
    public class UnlockJobCriteria
    {
        public List<string> JobIds { get; set; }
        public List<string> Mbls { get; set; }
        public List<string> CustomNos { get; set; }
        public List<string> Advances { get; set; }
        public List<string> Settlements { get; set; }
        public UnlockTypeEnum UnlockTypeNum { get; set; }
    }
}
