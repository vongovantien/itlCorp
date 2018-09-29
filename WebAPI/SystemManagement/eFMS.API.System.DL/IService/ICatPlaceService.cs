using SystemManagementAPI.Service.Models;
using ITL.NetCore.Connection.BL;
using SystemManagement.DL.Models;
using System.Collections.Generic;

namespace SystemManagement.DL.Services
{
    public interface ICatPlaceService : IRepositoryBase<CatPlace, CatPlaceModel>
    {
        List<CatPlaceModel> GetCatPlaceFollowHubAndBranch();
    }
}