using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ICatPlaceTypeService : IRepositoryBase<CatPlaceType, CatPlaceTypeModel>
    {
        IQueryable<CatPlaceTypeModel> Query(CatPlaceTypeCriteria criteria, string orderByProperty, bool isAscendingOrder);
    }
}
