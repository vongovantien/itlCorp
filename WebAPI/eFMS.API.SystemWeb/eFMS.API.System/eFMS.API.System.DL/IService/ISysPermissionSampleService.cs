using eFMS.API.Common.Globals;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysPermissionSampleService : IRepositoryBase<SysPermissionSample, SysPermissionSampleModel>
    {
        IQueryable<SysPermissionSampleModel> Query(SysPermissionGeneralCriteria criteria);
        IQueryable<SysPermissionSampleModel> Paging(SysPermissionGeneralCriteria criteria, int pageNumber, int pageSize, out int rowsCount);

        SysPermissionSampleModel GetBy(Guid? id);
        HandleState Update(SysPermissionSampleModel entity);
        HandleState Delete(Guid id);

        List<eFMS.API.System.DL.Common.CommonData> GetLevelPermissions();
    }
}
