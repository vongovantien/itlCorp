using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Catalogue.Service.ViewModels;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatPlaceService : IRepositoryBase<CatPlace, CatPlaceModel>
    {
        List<vw_catPlace> Query(CatPlaceCriteria criteria);
        List<vw_catPlace> Paging(CatPlaceCriteria criteria, int page, int size, out int rowsCount);
    }
}
