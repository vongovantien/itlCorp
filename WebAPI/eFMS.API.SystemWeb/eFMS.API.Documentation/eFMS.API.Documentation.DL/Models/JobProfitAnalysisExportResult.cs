using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class JobProfitAnalysisExportResult
    {
        public string JobNo { get; set; }
        public string Service { get; set; }
        public string Mbl { get; set; }
        public string Hbl { get; set; }
        public Guid Hblid { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public int? Quantity { get; set; }
        public int? Cont20 { get; set; }
        public int? Cont40 { get; set; }
        public int? Cont40HC { get; set; }
        public int? Cont45 { get; set; }
        public int? Cont { get; set; }
        public decimal? GW { get; set; }
        public decimal? CW { get; set; }
        public decimal? CBM { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeType { get; set; }
        public bool? isGroup { get; set; }
        //public decimal? Revenue { get; set; }
        //public decimal? Cost { get; set; }
        public decimal? JobProfit { get; set; }
        public decimal? TotalRevenue { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? TotalJobProfit { get; set; }
    }
}
