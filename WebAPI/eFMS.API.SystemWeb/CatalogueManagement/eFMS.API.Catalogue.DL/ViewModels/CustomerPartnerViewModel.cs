using eFMS.API.Catalogue.DL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CustomerPartnerViewModel
    {
        public string SalePersonId { get; set; }
        public string SalePersonName { get; set; }
        public int SumNumberPartner { get; set; }
        public List<CatPartnerViewModel> CatPartnerModels { get; set; }
    }
}
