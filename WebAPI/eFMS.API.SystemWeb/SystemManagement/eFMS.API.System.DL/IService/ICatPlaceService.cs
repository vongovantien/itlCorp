using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using eFMS.API.System.Service.ViewModels;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;

namespace eFMS.API.System.DL.IService
{
    public interface ICatPlaceService : IRepositoryBase<CatPlace, CatPlaceModel>
    {
        List<vw_catPlace> Query(CatPlaceCriteria criteria);
        List<vw_catPlace> Paging(CatPlaceCriteria criteria, int page, int size, out int rowsCount);
    }
}
