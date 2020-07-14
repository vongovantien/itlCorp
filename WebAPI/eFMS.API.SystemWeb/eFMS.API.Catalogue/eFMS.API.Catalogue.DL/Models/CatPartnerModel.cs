﻿using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPartnerModel: CatPartner
    {
        public List<CatContractModel> Contracts { get; set; }
        public string CountryName { get; set; }
        public string CountryShippingName { get; set; }
        public string ProvinceName { get; set; }
        public string ProvinceShippingName { get; set; }
        public string ContractType { get; set; }
        public string ContractNo { get; set; }

        public string ContractService { get; set; }
        public PermissionAllowBase Permission { get; set; }

    }
}
