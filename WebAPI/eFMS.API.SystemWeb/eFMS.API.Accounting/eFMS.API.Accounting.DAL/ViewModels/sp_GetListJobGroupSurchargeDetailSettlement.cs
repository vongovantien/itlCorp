using System;

namespace eFMS.API.Accounting.Service.ViewModels
{
    public class sp_GetListJobGroupSurchargeDetailSettlement
    {
        public string SettlementNo { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public Guid HblId { get; set; }
        public Guid ShipmentId { get; set; }
        public string Type { get; set; }
        public string CustomNo { get; set; }
        public string AdvanceNo { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public decimal? Balance { get; set; }
        public bool? IsLocked { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
