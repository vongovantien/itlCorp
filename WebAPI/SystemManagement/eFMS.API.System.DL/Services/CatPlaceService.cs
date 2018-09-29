using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ITL.NetCore.Connection.EF;
using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class CatPlaceService : RepositoryBase<CatPlace, CatPlaceModel>, ICatPlaceService
    {  
        public CatPlaceService(IContextBase<CatPlace> repository, IMapper mapper) : base(repository, mapper)
        {
        }
        public List<CatPlaceModel> GetCatPlaceFollowHubAndBranch()
        {
            return Get(x => x.PlaceTypeId == "Hub" || x.PlaceTypeId == "Branch").ToList(); 
        }

    }
}
