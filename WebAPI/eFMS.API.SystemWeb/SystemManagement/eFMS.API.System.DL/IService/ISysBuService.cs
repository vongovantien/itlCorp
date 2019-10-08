using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.System.DL.IService
{
    public interface ISysBuService : IRepositoryBase<SysBu, SysBuModel>
    {
        List<SysBuModel> Query(SysBuCriteria criteria);
        List<SysBuModel> Paging(SysBuCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<SysBuModel> GetAll();
        HandleState Add(SysBuAddModel sysBuModel);
        HandleState Update(SysBuAddModel sysBuModel);
        HandleState Delete(Guid id);
    }
}
