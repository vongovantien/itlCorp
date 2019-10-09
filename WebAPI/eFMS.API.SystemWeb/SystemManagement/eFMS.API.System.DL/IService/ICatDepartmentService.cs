using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.System.DL.IService
{
    public interface ICatDepartmentService : IRepositoryBase<CatDepartment, CatDepartmentModel>
    {
        IQueryable<CatDepartmentModel> QueryData(CatDepartmentCriteria criteria);

        IQueryable<CatDepartmentModel> Paging(CatDepartmentCriteria criteria, int page, int size, out int rowsCount);
    }

}
