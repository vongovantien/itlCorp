using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using System.Linq;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatChargeGroupService : IRepositoryBaseCache<CatChargeGroup, CatChargeGroupModel>
    {
    }
}
