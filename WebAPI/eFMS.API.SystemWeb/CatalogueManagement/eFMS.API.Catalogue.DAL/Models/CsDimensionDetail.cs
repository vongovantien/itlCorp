using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CsDimensionDetail
    {
        public Guid Id { get; set; }
        public Guid? AirWayBillId { get; set; }
        public Guid? Mblid { get; set; }
        public Guid? Hblid { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Package { get; set; }
        public decimal? Hw { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
