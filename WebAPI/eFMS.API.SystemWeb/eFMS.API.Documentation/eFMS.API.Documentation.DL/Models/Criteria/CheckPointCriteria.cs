using eFMS.API.Documentation.DL.IService;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class CheckPointCriteria
    {
        public List<CheckPointPartnerHBLDataGroup> Data { get; set; }
        public string TransactionType { get; set; }
        public string SettlementCode { get; set; }
        public CHECK_POINT_TYPE Type { get; set; }
    }

    public class CheckPointPartnerHBLDataGroup
    {
        public string PartnerId { get; set; }
        public Guid? HblId { get; set; }
    }
}
