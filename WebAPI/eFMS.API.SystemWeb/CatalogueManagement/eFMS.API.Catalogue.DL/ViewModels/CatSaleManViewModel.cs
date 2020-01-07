using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatSaleManViewModel
    {
        public string Id { get; set; }
        public string SaleManId { get; set; }
        public string Office { get; set; }
        public string Company { get; set; }
        public bool? Status { get; set; }
        public string PartnerId { get; set; }
        public string Service { get; set; }
        public string Description { get; set; }
        public DateTime? EffectDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string UserModified { get; set; }
        public string Username { get; set; }

    }
}
