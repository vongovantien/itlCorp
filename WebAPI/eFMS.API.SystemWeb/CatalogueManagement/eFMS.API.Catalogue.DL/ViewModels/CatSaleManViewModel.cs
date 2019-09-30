using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatSaleManViewModel
    {
        public string Id { get; set; }
        public string Saleman_EN { get; set; }
        public string Office { get; set; }
        public string Company { get; set; }
        public bool? Status { get; set; }
        public int? PartnerId { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
