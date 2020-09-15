using System;
namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class AdvanceInfo
    {
        public string AdvanceNo { get; set; }
        public Decimal? AdvanceAmount { get; set; }
        public Decimal? TotalAmount { get; set; }
        public string CustomNo { get; set; }
    }
}
