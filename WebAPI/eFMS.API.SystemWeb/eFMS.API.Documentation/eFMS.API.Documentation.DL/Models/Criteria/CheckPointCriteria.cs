using eFMS.API.Documentation.DL.IService;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class CheckPointCriteria
    {
        public List<string> PartnerIds { get; set; }
        public string Hblid { get; set; }
        public string TransactionType { get; set; }
        public string SettlementCode { get; set; }
        public CHECK_POINT_TYPE Type { get; set; }
    }
}
