using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Provider.Services.IService;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatPotentialService: IRepositoryBase<CatPotential, CatPotentialModel>, IPermissionBaseService<CatPotential, CatPotentialModel>
    {
        IQueryable<CatPotentialModel> Query(CatPotentialCriteria criteria);
        IQueryable<CatPotentialModel> Paging(CatPotentialCriteria criteria, int page, int size, out int rowsCount);
        HandleState Update(CatPotential model);
        HandleState AddNew(CatPotential model);
        HandleState Delete(Guid Id);
        CatPotentialEditModel GetDetail(Guid id);
        IQueryable<CatPotentialModel> QueryExport(CatPotentialCriteria criteria);
    }
}
