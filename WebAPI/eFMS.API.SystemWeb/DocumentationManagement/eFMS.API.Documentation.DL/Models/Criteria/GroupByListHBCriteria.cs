using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class GroupByListHBCriteria
    {
        public Guid Id { get; set; }
        public string partnerID { get; set; }
        public bool IsHouseBillID { get; set; }
        public List<GroupChargeModel> listData { get; set; }
    }
}
