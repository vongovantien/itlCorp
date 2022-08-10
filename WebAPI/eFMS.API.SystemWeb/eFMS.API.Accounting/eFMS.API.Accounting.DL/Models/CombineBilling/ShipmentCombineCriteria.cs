using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class ShipmentCombineCriteria
    {
        public string PartnerId { get; set; }
        public DateTime? IssuedDateFrom { get; set; }
        public DateTime? IssuedDateTo { get; set; }
        public DateTime? ServiceDateFrom { get; set; }
        public DateTime? ServiceDateTo { get; set; }
        public string Type { get; set; }
        public List<string> Services { get; set; }
        public string DocumentType { get; set; }
        public List<string> DocumentNo { get; set; }
        public string CombineNo { get; set; }
    }
}
