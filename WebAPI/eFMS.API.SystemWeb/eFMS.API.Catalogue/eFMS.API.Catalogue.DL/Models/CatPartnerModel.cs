using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPartnerModel: CatPartner
    {
        public List<CatContractModel> Contracts { get; set; }
        public List<CatPartnerEmailModel> PartnerEmails { get; set; }
        public string CountryName { get; set; }
        public string CountryShippingName { get; set; }
        public string ProvinceName { get; set; }
        public string ProvinceShippingName { get; set; }
        public string ContractType { get; set; }
        public string ContractNo { get; set; }
        public string SalesmanId { get; set; }

        public string ContractService { get; set; }
        public List<string> idsContract { get; set; }
        public string UserCreatedContract { get; set; }
        public PermissionAllowBase Permission { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
        public bool? IsRequestApproval { get; set; }
    }
}
