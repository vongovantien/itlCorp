using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatChargeModel : CatCharge
    {
        public string currency { get; set; }
        public string unit { get; set; }

        public PermissionAllowBase Permission { get; set; }
    }
}
