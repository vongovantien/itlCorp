using System;

namespace eFMS.API.Catalogue.Models
{
    public class CatContractEditModel
    {
        public Guid? Id { get; set; }
        public string SaleManId { get; set; }
        public Guid Office { get; set; }
        public Guid Company { get; set; }
        public string Service { get; set; }
        public bool? Status { get; set; }
        public string PartnerId { get; set; }
        public DateTime? EffectDate { get; set; }
        public string Description { get; set; }
        public string UserCreated { get; set; }
        public string FreightPayment { get; set; }
        public string ServiceName { get; set; }
        public DateTime? CreateDate { get; set; }

    }
}
