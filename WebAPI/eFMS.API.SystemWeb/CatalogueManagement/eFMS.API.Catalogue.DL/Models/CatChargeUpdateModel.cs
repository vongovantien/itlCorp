using eFMS.API.Catalogue.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatChargeUpdateModel
    {
        public CatCharge Charge { get; set; }
        public List<CatChargeDefaultAccount> ListChargeDefaultAccount { get; set; }
    }
}
