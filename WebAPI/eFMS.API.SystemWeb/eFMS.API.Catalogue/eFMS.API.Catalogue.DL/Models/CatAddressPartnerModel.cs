using eFMS.API.Catalogue.Service.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatAddressPartnerModel : CatAddressPartner
    {
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
    }
}
