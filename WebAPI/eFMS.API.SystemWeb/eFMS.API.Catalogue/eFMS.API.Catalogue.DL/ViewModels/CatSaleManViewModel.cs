using System;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatSaleManViewModel
    {
        public string Id { get; set; }
        public string SaleManId { get; set; }
        public Guid? Office { get; set; }
        public string OfficeName { get; set; }
        public Guid? Company { get; set; }
        public string CompanyName { get; set; }
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
        public string FreightPayment { get; set; }

    }
}
