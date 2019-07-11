using eFMS.API.Catalogue.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPartnerImportModel: CatPartner
    {
        public string DepartmentName { get; set; }
        public string SaleManName { get; set; }
        public string CountryBilling { get; set; }
        public string CityBilling { get; set; }
        public string CountryShipping { get; set; }
        public string CityShipping { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
        public string ACReference { get; set; }
        public string Profile { get; set; }
    }
}
