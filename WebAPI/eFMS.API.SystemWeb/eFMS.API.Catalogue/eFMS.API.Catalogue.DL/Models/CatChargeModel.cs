using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatChargeModel : CatCharge
    {
        public string currency { get; set; }
        public string unit { get; set; }
        public string OfficesName { get; set; }
        public string ChargeGroupName { get; set; }
        public string BuyingCode { get; set; }
        public string BuyingName { get; set; }
        public string SellingCode { get; set; }
        public string SellingName { get; set; }
        public PermissionAllowBase Permission { get; set; }
    }
}
