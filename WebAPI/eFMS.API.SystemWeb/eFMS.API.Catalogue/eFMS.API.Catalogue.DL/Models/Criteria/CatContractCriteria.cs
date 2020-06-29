using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatContractCriteria
    {
        public string All { get; set; }
        public string Saleman_ID { get; set; }
        public Guid Office { get; set; }
        public Guid Company { get; set; }
        public bool? Status { get; set; }
        public string Description { get; set; }
        public DateTime? EffectDate { get; set; }
        public string PartnerId { get; set; }
    }
}
