using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPlaceModel: CatPlace
    {
        public PermissionAllowBase Permission { get; set; }
    }
}
