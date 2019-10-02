using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatUnitService: IRepositoryBaseCache<CatUnit,CatUnitModel>
    {
        IQueryable<CatUnitModel> GetAll();
        IQueryable<CatUnit> Query(CatUnitCriteria criteria);
        List<CatUnit> Paging(CatUnitCriteria criteria, int pageNumber, int pageSize, out int rowsCount);

        List<UnitType> GetUnitTypes();
        HandleState Update(CatUnitModel model);
        HandleState Delete(short id);
    }
}
