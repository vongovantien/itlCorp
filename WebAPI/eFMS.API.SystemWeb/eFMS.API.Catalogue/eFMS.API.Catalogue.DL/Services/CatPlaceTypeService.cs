using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPlaceTypeService : RepositoryBase<CatPlaceType, CatPlaceTypeModel>, ICatPlaceTypeService
    {
        public CatPlaceTypeService(IContextBase<CatPlaceType> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
