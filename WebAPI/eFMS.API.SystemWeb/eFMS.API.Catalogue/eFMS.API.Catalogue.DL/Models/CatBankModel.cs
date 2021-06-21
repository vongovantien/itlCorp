using eFMS.API.Catalogue.Service.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatBankModel: CatBank
    {
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
    }
}
