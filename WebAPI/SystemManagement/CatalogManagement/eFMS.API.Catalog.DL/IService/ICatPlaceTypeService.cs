using eFMS.API.Catalog.DL.Models;
using eFMS.API.Catalog.DL.Models.Criteria;
using eFMS.API.Catalog.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalog.DL.IService
{
    public interface ICatPlaceTypeService : IRepositoryBase<CatPlaceType, CatPlaceTypeModel>
    {
        IQueryable<CatPlaceTypeModel> Query(CatPlaceTypeCriteria criteria, string orderByProperty, bool isAscendingOrder);
    }
}
