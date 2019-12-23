using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsDimensionDetail
    {
        public Guid Id { get; set; }
        public Guid? Mblid { get; set; }
        public Guid? Hblid { get; set; }
        public decimal? Lenght { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Package { get; set; }
        public decimal? Hw { get; set; }
        public decimal? Cbm { get; set; }
        public decimal? TotalHw { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
