using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.Models
{
    public class CatSaleManEditModel
    {
        public Guid? Id { get; set; }
        public string SaleManId { get; set; }
        public string Office { get; set; }
        public string Company { get; set; }
        public string Service { get; set; }
        public bool? Status { get; set; }
        public string PartnerId { get; set; }
        public DateTime? EffectDate { get; set; }
        public string Description { get; set; }
        public string UserCreated { get; set; }
    }
}
