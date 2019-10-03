using System;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatSaleMan
    {
        public CatSaleMan()
        {
            
        }
        public Guid Id { get; set; }
        public string Saleman_ID { get; set; }
        public string Office { get; set; }
        public string Company { get; set; }
        public string Service { get; set; }
        public bool? Status { get; set; }
        public string PartnerId { get; set; }
        public DateTime? EffectDate { get; set; }
        public string Description { get; set; }
        public DateTime? CreateDate { get; set; }
        public string UserModified { get; set; }
    }
}
