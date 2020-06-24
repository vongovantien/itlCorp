using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPartnerModel: CatPartner
    {
        public List<CatContractModel> contracts { get; set; }
        public string CountryName { get; set; }
        public string CountryShippingName { get; set; }
        public string ProvinceName { get; set; }
        public string ProvinceShippingName { get; set; }
        public PermissionAllowBase Permission { get; set; }

    }
}
