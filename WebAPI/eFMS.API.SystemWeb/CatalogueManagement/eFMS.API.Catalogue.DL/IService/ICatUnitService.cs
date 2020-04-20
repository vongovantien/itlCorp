﻿using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatUnitService: IRepositoryBaseCache<CatUnit,CatUnitModel>
    {
        IQueryable<CatUnitModel> Query(CatUnitCriteria criteria);
        IQueryable<CatUnitModel> Paging(CatUnitCriteria criteria, int pageNumber, int pageSize, out int rowsCount);

        List<UnitType> GetUnitTypes();
        HandleState Update(CatUnitModel model);
        HandleState Delete(short id);
        CatUnitModel GetDetail(short id);
    }
}
