using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPartnerModel: CatPartner
    {
        public List<CatSaleManModel> SaleMans { get; set; }
        public PermissionAllowBase Permission { get; set; }

    }
}
