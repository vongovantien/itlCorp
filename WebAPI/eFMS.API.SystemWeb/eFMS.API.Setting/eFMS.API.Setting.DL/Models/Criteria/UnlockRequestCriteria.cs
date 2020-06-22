using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models.Criteria
{
    public class UnlockRequestCriteria
    {
        public List<string> ReferenceNos { get; set; }
        public UnlockTypeEnum UnlockTypeNum { get; set; }
        public string Requester { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string StatusApproval { get; set; }
    }
}
